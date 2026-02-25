using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Events;
using AutodecisionCore.Extensions;
using AutodecisionCore.Services.Interfaces;
using AutodecisionCore.Utils;
using BmgMoney.MessageClient.Core.Interfaces;
using Confluent.Kafka;
using MassTransit;
using System.Diagnostics;
using Constants = AutodecisionCore.Contracts.Constants.Constants;

namespace AutodecisionCore.Services
{
    public class AutodecisionPublisherService : IAutodecisionPublisherService
    {
        private readonly ILogger<AutodecisionPublisherService> _logger;
        private readonly ITopicClient _producer;

        private readonly ITopicProducer<ProcessFinalValidation> _processFinalValidationProducer;
        private readonly ITopicProducer<ApproveApplicationRequestEvent> _approveApplicationRequestEventProducer;
        private readonly ITopicProducer<DeclineApplicationRequestEvent> _declineApplicationRequestEventProducer;
        private readonly ITopicProducer<RequireDocumentsRequestEvent> _requireDocumentsRequestEventProducer;
        private readonly ITopicProducer<NotifyDefaultDocumentsRequestEvent> _notifyDefaultDocumentsRequestEventProducer;
        private readonly ITopicProducer<RequireAllotmentRequestEvent> _requireAllotmentRequestEventProducer;

        public AutodecisionPublisherService(
            ILogger<AutodecisionPublisherService> logger,
            ITopicClient producer,
            ITopicProducer<ProcessFinalValidation> processFinalValidationProducer,
            ITopicProducer<ApproveApplicationRequestEvent> approveApplicationRequestEventProducer,
            ITopicProducer<DeclineApplicationRequestEvent> declineApplicationRequestEventProducer,
            ITopicProducer<RequireDocumentsRequestEvent> requireDocumentsRequestEventProducer,
            ITopicProducer<NotifyDefaultDocumentsRequestEvent> notifyDefaultDocumentsRequestEventProducer,
            ITopicProducer<RequireAllotmentRequestEvent> requireAllotmentRequestEventProducer
            )
        {
            _logger = logger;
            _producer = producer;
            _processFinalValidationProducer = processFinalValidationProducer;
            _approveApplicationRequestEventProducer = approveApplicationRequestEventProducer;
            _declineApplicationRequestEventProducer = declineApplicationRequestEventProducer;
            _requireDocumentsRequestEventProducer = requireDocumentsRequestEventProducer;
            _notifyDefaultDocumentsRequestEventProducer = notifyDefaultDocumentsRequestEventProducer;
            _requireAllotmentRequestEventProducer = requireAllotmentRequestEventProducer;
        }

        public async Task PublishAutodecisionProcessRequest(Application application, string reason)
        {
            _logger.LogInformation($"LoanNumber: {application.LoanNumber} requesting Process Application with reason: {reason}");

            var message = new Message<Null, ProcessApplicationRequestEvent>()
            {
                Value = new ProcessApplicationRequestEvent()
                {
                    ApplicationId = application.Id,
                    LoanNumber = application.LoanNumber,
                    Reason = reason,
                    RequestedAt = DateTimeUtil.Now
                }
            };

            await _producer.Produce("autodecision-process", message);

            _logger.LogInformation($"LoanNumber: {application.LoanNumber} had finished request for Process Application with reason: {reason}");
        }

        public async Task PublishTheFlagsRequest(ApplicationCore applicationCore, string redisKey, string reason)
        {
            var request = new ProcessFlagRequestEvent()
            {
                LoanNumber = applicationCore.LoanNumber,
                Key = redisKey,
                Version = applicationCore.ProcessingVersion,
                Reason = reason
            };

            var topicMessages = new Dictionary<string, Message<Null, ProcessFlagRequestEvent>[]>();

            foreach (var applicationFlag in applicationCore.GetNeededFlagsToProcess())
            {
                topicMessages.Add($"{Constants.Topics.FlagRequestPrefix}{applicationFlag.FlagCode}", new Message<Null, ProcessFlagRequestEvent>[]
                {
                    new Message<Null, ProcessFlagRequestEvent>() { Value = request }
                });
            }

            _logger.LogInformation($"LoanNumber: {applicationCore.LoanNumber} has started sending request for Flags");
            _producer.Produce<Null, ProcessFlagRequestEvent>(topicMessages);
            _logger.LogInformation($"LoanNumber: {applicationCore.LoanNumber} had finished sending request for Flags");

        }

        public async Task PublishFinalEvaluationRequest(ProcessFlagResponseEvent response)
        {
            var request = new ProcessFinalValidation()
            {
                LoanNumber = response.LoanNumber,
                Key = $"{Constants.Topics.RedisKeyPrefix}{response.LoanNumber}",
                Version = response.Version,
                Reason = response.Reason
            };

            _logger.LogInformation($"LoanNumber: {response.LoanNumber} initiated final request for Flag: {response.FlagCode}");

            var timer = new Stopwatch();
            timer.Start();

            await _processFinalValidationProducer.Produce(request);

            timer.Stop();
            TimeSpan timeTaken = timer.Elapsed;

            _logger.LogInformation($"Time to publish ProcessFinalValidation: {timeTaken.ToString(@"m\:ss\.fff")}. Loan Number: {response.LoanNumber}");

            _logger.LogInformation($"LoanNumber: {response.LoanNumber} completed final request request for Flag: {response.FlagCode}");
        }

        public async Task PublishFinalEvaluationRequest(ApplicationCore application, string reason)
        {
            _logger.LogInformation($"LoanNumber: {application.LoanNumber} initiated final request");
            var timer = new Stopwatch();
            timer.Start();

            var request = new ProcessFinalValidation()
            {
                LoanNumber = application.LoanNumber,
                Key = $"{Constants.Topics.RedisKeyPrefix}{application.LoanNumber}",
                Version = application.ProcessingVersion,
                Reason = reason
            };

            await _processFinalValidationProducer.Produce(request);

            timer.Stop();
            TimeSpan timeTaken = timer.Elapsed;

            _logger.LogInformation($"LoanNumber: {application.LoanNumber} completed final request request. Time Needed: {timeTaken.ToString(@"m\:ss\.fff")}");
        }

        public async Task PublishApproveApplicationRequestEvent(string loanNumber)
        {
            var request = new ApproveApplicationRequestEvent()
            {
                LoanNumber = loanNumber
            };

            _logger.LogInformation($"LoanNumber: {loanNumber} has started the request to approve the application");

            await _approveApplicationRequestEventProducer.Produce(request);

            //await _producer.Produce<Null, ApproveApplicationRequestEvent>(
            //    $"{Constants.Topics.NotifyAprove}", new Message<Null, ApproveApplicationRequestEvent>() { Value = request });

            _logger.LogInformation($"LoanNumber: {loanNumber} had finished the request to approve the application");
        }

        public async Task PublishDeclineApplicationRequestEvent(string loanNumber, string reason)
        {
            var request = new DeclineApplicationRequestEvent()
            {
                LoanNumber = loanNumber,
                Reason = reason
            };

            _logger.LogInformation($"LoanNumber: {loanNumber} has started the request to decline the application");


            await _declineApplicationRequestEventProducer.Produce(request);

            //await _producer.Produce<Null, DeclineApplicationRequestEvent>(
            //    $"{Constants.Topics.NotifyDecline}", new Message<Null, DeclineApplicationRequestEvent>() { Value = request });

            _logger.LogInformation($"LoanNumber: {loanNumber} had finished the request to decline the application");
        }

        public async Task PublishRequireDocumentsRequestEvent(string loanNumber, List<string> flagCodes)
        {
            var request = new RequireDocumentsRequestEvent()
            {
                LoanNumber = loanNumber,
                Flags = flagCodes
            };

            _logger.LogInformation($"LoanNumber: {loanNumber} has started the request to require documents");

            await _requireDocumentsRequestEventProducer.Produce(request);

            //await _producer.Produce<Null, RequireDocumentsRequestEvent>(
            //    $"{Constants.Topics.NotifyPending}", new Message<Null, RequireDocumentsRequestEvent>() { Value = request });

            _logger.LogInformation($"LoanNumber: {loanNumber} had finished the request to require documents");
        }

        public async Task PublishNotifyDefaultDocumentsRequestEvent(string loanNumber)
        {
            var request = new NotifyDefaultDocumentsRequestEvent()
            {
                LoanNumber = loanNumber,
            };

            _logger.LogInformation($"LoanNumber: {loanNumber} has started the request to notify default documents");

            await _notifyDefaultDocumentsRequestEventProducer.Produce(request);

            //await _producer.Produce<Null, NotifyDefaultDocumentsRequestEvent>(
            //    $"{Constants.Topics.NotifyPendingDefaultDocuments}", new Message<Null, NotifyDefaultDocumentsRequestEvent>() { Value = request });

            _logger.LogInformation($"LoanNumber: {loanNumber} had finished the request to notify default documents");
        }

        public async Task PublishRequireAllotmentRequestEvent(string loanNumber, string paymentType)
        {
            var request = new RequireAllotmentRequestEvent()
            {
                LoanNumber = loanNumber,
                PaymentType = paymentType
            };

            _logger.LogInformation($"LoanNumber: {loanNumber} has started the request to require allotment");

            await _requireAllotmentRequestEventProducer.Produce(request);

            //await _producer.Produce<Null, RequireAllotmentRequestEvent>(
            //    $"{Constants.Topics.NotifyAllotment}", new Message<Null, RequireAllotmentRequestEvent>() { Value = request });

            _logger.LogInformation($"LoanNumber: {loanNumber} had finished the request to require allotment");
        }

        public async Task HandleApplicationPublisher(string reason, Application application, ApplicationCore applicationCore, string redisKey)
        {
            if (application.ProductId == ApplicationProductId.Cashless && application.PaymentType == PayrollType.DebitCard)
            {
                await PublishFinalEvaluationRequest(applicationCore, reason);
                return;
            }
            if (reason == Reason.AllotmentSDDReceived && !applicationCore.HasPendingApprovalFlagsBesides(FlagCode.AllotmentValidation))
            {
                await PublishFinalEvaluationRequest(applicationCore, reason);
                return;
            }
            if (reason == Reason.AllFlagsApproved && !applicationCore.HasPendingApprovalFlagsBesides(new[] { FlagCode.LoanVerification, FlagCode.Flag209 }))
            {
                await PublishFinalEvaluationRequest(applicationCore, reason);
                return;
            }
            await PublishTheFlagsRequest(applicationCore, redisKey, reason);
        }

        public async Task PublishOpenConnectionsProcessRequest(ApplicationCore applicationCore, string reason)
        {
            _logger.LogInformation($"LoanNumber: {applicationCore.LoanNumber} sending message to Open Connections with reason: {reason}");

            var message = new Message<Null, OpenConnectionRequestEvent>()
            {
                Value = new OpenConnectionRequestEvent()
                {
                    ApplicationId = applicationCore.Id,
                    LoanNumber = applicationCore.LoanNumber,
                    Reason = reason,
                    RequestedAt = DateTimeUtil.Now
                }
            };

            await _producer.Produce(Topics.OpenConnections, message);

            _logger.LogInformation($"LoanNumber: {applicationCore.LoanNumber} had finished request for Open Connections with reason: {reason}");
        }
    }
}