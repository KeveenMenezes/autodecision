using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Data.Repositories.Interfaces;
using AutodecisionCore.DTOs;
using AutodecisionCore.Extensions;
using AutodecisionCore.Services.Interfaces;
using BMGMoney.SDK.V2.Cache.Redis;
using MassTransit;
using Newtonsoft.Json;
using RedLockNet;
using System.Diagnostics;
using Application = AutodecisionCore.Contracts.ViewModels.Application.Application;
using Constants = AutodecisionCore.Contracts.Constants.Constants;

namespace AutodecisionCore.Handlers
{
    public class AutodecisionProcessHandler : IConsumer<ProcessApplicationRequestEvent>
    {
        private readonly ILogger<AutodecisionProcessHandler> _logger;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IAutodecisionCompositeService _autodecisionCompositeService;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IApplicationCoreService _applicationCoreService;
        private readonly IExternalValidationService _externalValidationService;
        private readonly IAutodecisionPublisherService _autodecisionPublisherService;
        private readonly IApplicationCoreRepository _applicationCoreRepository;
        private readonly IHandlerHelper _handlerHelper;
        private readonly IDistributedLockFactory _redLockFactory;
        private readonly IApplicationFlagsService _applicationFlagsService;

        public AutodecisionProcessHandler(
            ILogger<AutodecisionProcessHandler> logger,
            IAutodecisionCompositeService autodecisionCompositeService,
            IRedisCacheService redisCacheService,
            IApplicationCoreService applicationCoreService,
            IApplicationRepository applicationRepository,
            IExternalValidationService externalValidationService,
            IAutodecisionPublisherService autodecisionPublisherService,
            IApplicationCoreRepository applicationCoreRepository,
            IHandlerHelper handlerHelper,
            IDistributedLockFactory redLockFactory,
            IApplicationFlagsService applicationFlagsService
            )
        {
            _logger = logger;
            _autodecisionCompositeService = autodecisionCompositeService;
            _redisCacheService = redisCacheService;
            _applicationCoreService = applicationCoreService;
            _applicationRepository = applicationRepository;
            _externalValidationService = externalValidationService;
            _autodecisionPublisherService = autodecisionPublisherService;
            _applicationCoreRepository = applicationCoreRepository;
            _handlerHelper = handlerHelper;
            _redLockFactory = redLockFactory;
            _applicationFlagsService = applicationFlagsService;
        }

        public async Task Consume(ConsumeContext<ProcessApplicationRequestEvent> context)
        {
            var stopwatch = Stopwatch.StartNew();
            var totalStopWatch = Stopwatch.StartNew();
            var recordedTimes = new Dictionary<string, TimeSpan>();

            totalStopWatch.Start();
            stopwatch.Start();

            if (!_handlerHelper.ValidateProperty(context.Message.LoanNumber, "Process Application Request Event without LoanNumber")) return;
            _logger.LogInformation($"Starting process loanNumber: {context.Message.LoanNumber} - Reason: {context.Message.Reason}");

            var loanNumber = context.Message.LoanNumber;
            var resource = $"{RedLockKeys.RequestProcess}{loanNumber}";
            var redisKey = $"{Constants.Topics.RedisKeyPrefix}{loanNumber}";
            var timeResouce = new TimeSpan(0, 0, 2, 0);
            var retryTime = new TimeSpan(0, 0, 0, 3);

            using (var redLock = await _redLockFactory.CreateLockAsync(resource, timeResouce, timeResouce, retryTime))
            {
                if (!_handlerHelper.HasRedLockKeyBeenAcquired(redLock.IsAcquired, resource, loanNumber, "Autodecision Process")) return;

                var applicationCore = await _applicationCoreService.ApplicationCoreRegister(context.Message.LoanNumber);
                recordedTimes.Add("ApplicationCoreRegister", stopwatch.Elapsed);

                if (!IsProcessingRequestAllowed(applicationCore, context.Message)) return;
                stopwatch.Restart();

                var application = await _applicationRepository.GetApplicationInfo(loanNumber);
                recordedTimes.Add("GetApplication", stopwatch.Elapsed);

                if (!_handlerHelper.IsApplicationProcessable(application, loanNumber)) return;
                stopwatch.Restart();

                await _applicationFlagsService.HandleApplicationFlags(application, applicationCore, context.Message.Reason);
                recordedTimes.Add("HandleApplicationFlags", stopwatch.Elapsed);
                stopwatch.Restart();

                await _autodecisionPublisherService.PublishNotifyDefaultDocumentsRequestEvent(application.LoanNumber);
                recordedTimes.Add("PublishDocuments", stopwatch.Elapsed);
                stopwatch.Restart();

                await _externalValidationService.Run(application);
                recordedTimes.Add("RunExternalServices", stopwatch.Elapsed);

                applicationCore.RefreshProcessingInfo();
                stopwatch.Restart();

                var timing = await CollectAndSaveCacheInformation(application, applicationCore, redisKey, stopwatch);
                stopwatch.Restart();

                try
                {
                    await _applicationCoreRepository.SaveChanges();
                    recordedTimes.Add("SaveChangesSuccess", stopwatch.Elapsed);
                    stopwatch.Restart();

                }
                catch (Exception ex)
                {
                    recordedTimes.Add("SaveChangesError", stopwatch.Elapsed);
                    throw;
                }

                recordedTimes.Add("SaveApplicationCoreChanges", stopwatch.Elapsed);
                stopwatch.Restart();

                await _autodecisionPublisherService.HandleApplicationPublisher(context.Message.Reason, application, applicationCore, redisKey);
                LogElapsedTimes(application.LoanNumber, recordedTimes, timing, stopwatch, totalStopWatch);
            }
        }

        private async Task<RedisInformationTiming> CollectAndSaveCacheInformation(Application application, ApplicationCore applicationCore, string redisKey, Stopwatch stopwatch)
        {
            try
            {
                _logger.LogInformation($"Starting to collect information to loanNumber: {application.LoanNumber}");

                var redisContent = await _autodecisionCompositeService.FillCompositeData(application, applicationCore);
                var elapsedSearchRedisData = stopwatch.Elapsed;

                applicationCore.SetCustomerInfo($"{redisContent.Customer.FirstName} {redisContent.Customer.LastName}", redisContent.Application.EmployerName, redisContent.Application.StateAbbreviation);
                applicationCore.SetApplicationInfo(application.Type);

                await _redisCacheService.StringSetAsync(redisKey, JsonConvert.SerializeObject(redisContent));
                var elapsedSetRedisData = stopwatch.Elapsed - elapsedSearchRedisData;

                _logger.LogInformation($"Saving information collected at Cache to loanNumber: {application.LoanNumber}");
                return new RedisInformationTiming(elapsedSearchRedisData, elapsedSetRedisData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving cached information for loanNumber: {application.LoanNumber} | Error: {ex.Message} ");
                throw;
            }
        }

        private void LogElapsedTimes(string loanNumber, Dictionary<string, TimeSpan> recordedTimes, RedisInformationTiming timing, Stopwatch stopwatch, Stopwatch totalStopWatch)
        {
            stopwatch.Stop();
            totalStopWatch.Stop();

            _logger.LogInformation($"LoanNumber: {loanNumber} --- Time for: " +
                                   $"RegisterApplicationCore: {recordedTimes["ApplicationCoreRegister"].ToString(@"m\:ss\.fff")} | " +
                                   $"ApplicationRepository: {recordedTimes["GetApplication"].ToString(@"m\:ss\.fff")} | " +
                                   $"BindOrReprocessApplicationFlags: {recordedTimes["HandleApplicationFlags"].ToString(@"m\:ss\.fff")} | " +
                                   $"PublishDefaultDocuments: {recordedTimes["PublishDocuments"].ToString(@"m\:ss\.fff")} | " +
                                   $"RunExternalServices: {recordedTimes["RunExternalServices"].ToString(@"m\:ss\.fff")} |" +
                                   $"SearchRedisInformation: {timing.ElapsedSearchRedisData.ToString(@"m\:ss\.fff")} |" +
                                   $"SaveRedisInformation: {timing.ElapsedSetRedisData.ToString(@"m\:ss\.fff")} | " +
                                   $"SavingAutodecisionCore: {recordedTimes["SaveApplicationCoreChanges"].ToString(@"m\:ss\.fff")} | " +
                                   $"PublishFlagsRequest / FinalEvaluation: {stopwatch.Elapsed.ToString(@"m\:ss\.fff")} | " +
                                   $"AutoDecisionProcessTheEntireHandler: {totalStopWatch.Elapsed.ToString(@"m\:ss\.fff")}");
        }

        private bool IsProcessingRequestAllowed(ApplicationCore applicationCore, ProcessApplicationRequestEvent request)
        {
            if (!applicationCore.AllowedProcessing(request.RequestedAt))
            {
                _logger.LogInformation($"LoanNumber: {applicationCore.LoanNumber} with Reason: {request.Reason} not allowed to process due to request time");
                return false;
            }
            return true;
        }
    }
}