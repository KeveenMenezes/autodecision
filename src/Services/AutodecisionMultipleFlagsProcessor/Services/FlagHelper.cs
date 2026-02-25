using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Contracts.ViewModels.OpenPayrollData;
using AutodecisionMultipleFlagsProcessor.Utility;
using BMGMoney.SDK.V2.Cache.Redis;
using MassTransit;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace AutodecisionMultipleFlagsProcessor.Services
{
    public interface IFlagHelper
    {
        Task<AutodecisionCompositeData> GetAutodecisionCompositeData(string key);
        Task SendReponseMessage(ProcessFlagResponseEvent response);
        ProcessFlagResponseEvent BuildFlagResponse(string flagId, AutodecisionCompositeData compositeData);
        ProcessFlagResponseEvent BuildFlagResponse(string flagId, AutodecisionCompositeData compositeData, FlagResultEnum flagResult);
        bool IsValidVersion(ProcessFlagRequestEvent message, AutodecisionCompositeData compositeData, string flagCode, ILogger logger);
        void RaiseFlag(ProcessFlagResponseEvent response, string description);
        (bool isSuccess, ProcessFlagResponseEvent response) ValidOpenBankingAccount(AutodecisionCompositeData autodecisionCompositeData, ProcessFlagResponseEvent response, bool shouldConnectOpenBanking);
        (bool isSuccess, ProcessFlagResponseEvent response) ValidOpenPayrollRemainderAccount(AutodecisionCompositeData autodecisionCompositeData, ProcessFlagResponseEvent response, bool shouldConnectOpenPayroll);
    }

    public class FlagHelper : IFlagHelper
    {
        private readonly IRedisCacheService _redisCacheService;
        private readonly ILogger<FlagHelper> _logger;
        private readonly ITopicProducer<ProcessFlagResponseEvent> _responseProducer;

        public FlagHelper(IRedisCacheService redisCacheService, ILogger<FlagHelper> logger, ITopicProducer<ProcessFlagResponseEvent> responseProducer)
        {
            _redisCacheService = redisCacheService;
            _logger = logger;
            _responseProducer = responseProducer;
        }

        public async Task<AutodecisionCompositeData> GetAutodecisionCompositeData(string key)
        {
            var cacheContent = await _redisCacheService.StringGetAsync(key);

            if (string.IsNullOrEmpty(cacheContent))
            {
                _logger.LogError($"Error at redis. Key:{key}");
                throw new Exception("Error at redis");
            }

            return JsonConvert.DeserializeObject<AutodecisionCompositeData>(cacheContent);
        }


        public async Task SendReponseMessage(ProcessFlagResponseEvent response)
        {
            _logger.LogInformation(JsonConvert.SerializeObject(response));
            await _responseProducer.Produce(response);
        }

        public ProcessFlagResponseEvent BuildFlagResponse(string flagId, AutodecisionCompositeData compositeData)
        {
            return new ProcessFlagResponseEvent()
            {
                FlagCode = flagId,
                ProcessedAt = DateTimeUtil.Now,
                LoanNumber = compositeData.Application.LoanNumber,
                Version = compositeData.Version
            };
        }

        public ProcessFlagResponseEvent BuildFlagResponse(string flagId, AutodecisionCompositeData compositeData, FlagResultEnum flagResult)
        {
            return new ProcessFlagResponseEvent()
            {
                FlagCode = flagId,
                ProcessedAt = DateTimeUtil.Now,
                LoanNumber = compositeData.Application.LoanNumber,
                FlagResult = flagResult,
                Version = compositeData.Version
            };
        }

        public bool IsValidVersion(ProcessFlagRequestEvent message, AutodecisionCompositeData compositeData, string flagCode, ILogger logger)
        {
            if (message.Version.HasValue)
            {
                if (message.Version != compositeData.Version)
                {
                    logger.LogInformation($"Flag: {flagCode} Rejected. Old processing version. Loan Number: {message.LoanNumber}");
                    return false;
                }
            }

            return true;
        }

        public void RaiseFlag(ProcessFlagResponseEvent response, string description)
        {
            response.Message = description;
            response.FlagResult = FlagResultEnum.PendingApproval;
        }

        public (bool isSuccess, ProcessFlagResponseEvent response) ValidOpenBankingAccount(AutodecisionCompositeData autodecisionCompositeData, ProcessFlagResponseEvent response, bool shouldConnectOpenBanking)
        {
            var defaultOpenBankingAccount = autodecisionCompositeData.OpenBanking?.Connections?.Where(x => x.IsDefault).FirstOrDefault();
            if (!shouldConnectOpenBanking && defaultOpenBankingAccount is null)
            {
                return (true, response);
            }
            if (shouldConnectOpenBanking && defaultOpenBankingAccount is null)
            {
                RaiseFlag(response, $"Open Banking has no default account connected");
                return (false, response);
            }
            if (!CompareValues(defaultOpenBankingAccount.RoutingNumber, autodecisionCompositeData.Application.BankRoutingNumber))
            {
                RaiseFlag(response, $"Open Banking routing number does not match customer bank routing number!");
                return (false, response);
            }
            if (!CompareValues(defaultOpenBankingAccount.AccountNumber, autodecisionCompositeData.Application.BankAccountNumber))
            {
                RaiseFlag(response, $"Open Banking account number does not match customer bank account number!");
                return (false, response);
            }
            return (true, response);
        }

        public (bool isSuccess, ProcessFlagResponseEvent response) ValidOpenPayrollRemainderAccount(AutodecisionCompositeData autodecisionCompositeData, ProcessFlagResponseEvent response, bool shouldConnectOpenPayroll)
        {
            var remainderAccount = autodecisionCompositeData.OpenPayroll?.Connections.OrderByDescending(x => x.ConnectedAt)
                .Where(x => x.IsActive).SelectMany(x => x.PayAllocations).FirstOrDefault(x => x.IsRemainder);

            if (!shouldConnectOpenPayroll && remainderAccount is null)
            {
                return (true, response);
            }
            if (shouldConnectOpenPayroll && remainderAccount is null)
            {
                RaiseFlag(response, $"Open Payroll Remainder not found");
                return (false, response);
            }


            if (!CompareValues(remainderAccount.RoutingNumber, autodecisionCompositeData.Application.BankRoutingNumber))
            {
                RaiseFlag(response, $"Open Payroll remainder routing number does not match customer bank routing number!");
                return (false, response);
            }

            if (!CompareValues(remainderAccount.AccountNumber, autodecisionCompositeData.Application.BankAccountNumber))
            {
                RaiseFlag(response, $"Open Payroll remainder account number does not match customer bank account number!");
                return (false, response);
            }
            return (true, response);
        }

        private bool isCompareValuesValid(string first, string second)
        {
            return !string.IsNullOrWhiteSpace(first)
                && !string.IsNullOrWhiteSpace(second);
        }

        private bool CompareValues(string mainSource, string applicationSource)
        {
            if (!this.isCompareValuesValid(mainSource, applicationSource))
                return false;

			var mainSourceFormatted = Regex.Match(mainSource, @"\d+").Value;

            return applicationSource.EndsWith(mainSourceFormatted);
		}
    }
}
