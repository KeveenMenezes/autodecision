using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Data.Repositories.Interfaces;
using AutodecisionCore.Services.Interfaces;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using BMGMoney.SDK.V2.Cache.Redis;
using Newtonsoft.Json;

namespace AutodecisionCore.Services
{
    public class AutodecisionCompositeService : IAutodecisionCompositeService
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IApplicationCoreRepository _applicationCoreRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ICensusRepository _censusRepository;
        private readonly IOpenPayrollRepository _openPayrollRepository;
        private readonly IOpenBankingRepository _openBankingRepository;
        private readonly IFactorTrustRepository _factorTrustRepository;
        private readonly ILogger<AutodecisionCompositeService> _logger;
        private readonly IRedisCacheService _redisCacheService;
        private readonly ITransunionRepository _transunionRepository;
        private readonly IBlockListRepository _blockListRepository;
        private readonly IWhiteListRepository _whiteListRepository;
        private readonly IDebitCardRepository _debitCardRepository;
        private readonly IClarityRepository _clarityRepository;
        private readonly IApplicationWarningRepository _applicationWarningRepository;
        private readonly IFacetecRepository _facetecRepository;
        private readonly INewCreditPolicyRepository _newCreditPolicyRepository;
        private readonly IFeatureToggleClient _featureToggleClient;
        private readonly IFlagHelperRepository _flagHelperRepository;
        private readonly INewOpenBankingRepository _newOpenBankingRepository;
        private readonly ICreditRiskRepository _creditRiskRepository;
        private readonly ICreditRiskService _creditRiskService;
        private readonly IEmployerRepository _employerRepository;

        public AutodecisionCompositeService(
            IApplicationRepository applicationRepository,
            IApplicationCoreRepository applicationCoreRepository,
            ICustomerRepository customerRepository,
            ICensusRepository censusRepository,
            IOpenPayrollRepository openPayrollRepository,
            IOpenBankingRepository openBankingRepository,
            IFactorTrustRepository factorTrustRepository,
            ILogger<AutodecisionCompositeService> logger,
            IRedisCacheService redisCacheService,
            IFacetecRepository facetecRepository,
            IDebitCardRepository debitCardRepository,
            ITransunionRepository transunionRepository,
            IBlockListRepository blockListRepository,
            IWhiteListRepository whiteListRepository,
            IClarityRepository clarityRepository,
            IApplicationWarningRepository applicationWarningRepository,
            INewCreditPolicyRepository newCreditPolicyRepository,
            IFeatureToggleClient featureToggleClient,
            INewOpenBankingRepository newOpenBankingRepository,
            IFlagHelperRepository flagHelperRepository,
            ICreditRiskRepository creditRiskRepository,
            ICreditRiskService creditRiskService,
            IEmployerRepository employerRepository)
        {
            _applicationRepository = applicationRepository;
            _applicationCoreRepository = applicationCoreRepository;
            _customerRepository = customerRepository;
            _censusRepository = censusRepository;
            _openPayrollRepository = openPayrollRepository;
            _openBankingRepository = openBankingRepository;
            _factorTrustRepository = factorTrustRepository;
            _logger = logger;
            _redisCacheService = redisCacheService;
            _debitCardRepository = debitCardRepository;
            _transunionRepository = transunionRepository;
            _blockListRepository = blockListRepository;
            _whiteListRepository = whiteListRepository;
            _clarityRepository = clarityRepository;
            _applicationWarningRepository = applicationWarningRepository;
            _facetecRepository = facetecRepository;
            _newCreditPolicyRepository = newCreditPolicyRepository;
            _featureToggleClient = featureToggleClient;
            _newOpenBankingRepository = newOpenBankingRepository;
            _flagHelperRepository = flagHelperRepository;
            _creditRiskRepository = creditRiskRepository;
            _creditRiskService = creditRiskService;
            _employerRepository = employerRepository;
        }

        public async Task<AutodecisionCompositeData> FillCompositeData(Application application, ApplicationCore applicationCore)
        {
            OpenBanking openBanking;
            TotalIncome totalIncome = new TotalIncome();
            DebitCard debitCard = new DebitCard();
            CreditRisk creditRisk = new CreditRisk();


            _logger.LogInformation($"Loading customer information {application.LoanNumber}");
            Customer customer = await _customerRepository.GetCustomerInfo(application.CustomerId);

            _logger.LogInformation($"Loading lastApplications information {application.LoanNumber}");
            var lastApplications = await _applicationRepository.GetLastApplications(application.CustomerId, application.LoanNumber);

            if (lastApplications != null)
            {
                foreach (var lastApplication in lastApplications)
                {
                    lastApplication.HasCustomerIdentiyValidated = await _applicationCoreRepository.HasCustomerIdentityFlagNotRaised(lastApplication.LoanNumber);
                }
            }

            _logger.LogInformation($"Loading census information {application.LoanNumber}");
            var census = await _censusRepository.GetCensusDataByCustomerId(application.CustomerId, application.EmployerId);

            _logger.LogInformation($"Loading openPayroll information {application.LoanNumber}");
            var openPayroll = await _openPayrollRepository.GetOpenPayrollConnections(application.CustomerId, application.Id, application.SubmittedAt);

            _logger.LogInformation($"Loading creditPolicy information {application.LoanNumber}");
            var creditPolicy = await _newCreditPolicyRepository.GetNewCreditPolicyAndRules(application);

            _logger.LogInformation($"Loading openBanking information {application.LoanNumber}");

            if (_featureToggleClient.IsEnabled("new-open-banking-enabled"))
                openBanking = await _newOpenBankingRepository.GetNewOpenBanking(application.CustomerId);
            else
                openBanking = await _openBankingRepository.GetOpenBankingInfo(application.CustomerId);

            _logger.LogInformation($"Loading factorTrust information {application.LoanNumber}");
            var factorTrust = await _factorTrustRepository.GetFactorTrustInfo(application.CustomerId, application.LoanNumber);

            if (application.FundingMethod == "debit_card" || application.PaymentType == "debit_card")
            {
                _logger.LogInformation($"Loading debitCard information {application.LoanNumber}");
                var debitCardInfo = await _debitCardRepository.GetAccountsByCustomerId(application.CustomerId);
                if (debitCardInfo != null && debitCardInfo.Active)
                {
                    debitCard = await _debitCardRepository.BinNameLink(debitCardInfo, customer.BankName);
                }
            }

            _logger.LogInformation($"Loading facetec information {application.LoanNumber}");
            var faceRecognition = await _facetecRepository.GetFaceRecognition(customer.Id);

            _logger.LogInformation($"Loading TransunionResult information {application.LoanNumber}");
            var transunionResult = await _transunionRepository.GetTransunionResultInfo(application.CustomerId);

            _logger.LogInformation($"Loading Block List information {application.LoanNumber}");
            var blockList = await _blockListRepository.GetBlockListInfo(application.CustomerId);

            _logger.LogInformation($"Loading White List information {application.LoanNumber}");
            var whiteList = await _whiteListRepository.GetWhiteListInfo(application.CustomerId);

            _logger.LogInformation($"Loading Clarity information {application.LoanNumber}");
            var clarity = await _clarityRepository.GetClarityInfo(application.LoanNumber);

            _logger.LogInformation($"Loading Application Warnings information {application.LoanNumber}");
            var applicationWarnings = await _applicationWarningRepository.GetApplicationWarningsInfo(application.Id);

            _logger.LogInformation($"Loading Flag Helper {application.EmployerId}");
            var flagValidatorHelper = await _flagHelperRepository.GetFlagHelperInformationAsync(application.CustomerId, application.EmployerId, application.Id);

            _logger.LogInformation($"Loading Employer information {application.EmployerId}");
            var employer = await _employerRepository.GetEmployerAsync(application.EmployerId);

            if (_creditRiskService.ValidateAllowedLoanToCalculateScore(application, employer))
            {
                _logger.LogInformation($"Loading Credit Risk information {application.LoanNumber}");
                creditRisk.ApplicationScore = await _creditRiskRepository.GetApplicationScore(application.LoanNumber);
            }

            if (_featureToggleClient.IsEnabled("FeatureLineAssignment"))
            {
                _logger.LogInformation($"Loading Income information {application.LoanNumber} - Customer Id {application.CustomerId}");
                totalIncome = await _applicationRepository.GetTotalIncomeDetailsByApplicationIdAsync(application.Id);
            }

            _logger.LogInformation($"Loading information finished {application.LoanNumber}");

            return new AutodecisionCompositeData
            {
                Application = application,
                LastApplications = lastApplications,
                Customer = customer,
                Census = census,
                OpenPayroll = openPayroll,
                CreditPolicy = creditPolicy,
                OpenBanking = openBanking,
                FactorTrust = factorTrust,
                DebitCard = debitCard,
                TransunionResult = transunionResult,
                BlockList = blockList,
                WhiteList = whiteList,
                Clarity = clarity,
                ApplicationWarnings = applicationWarnings,
                FaceRecognition = faceRecognition,
                FlagValidatorHelper = flagValidatorHelper,
                Version = applicationCore.ProcessingVersion,
                CreditRisk = creditRisk,
                Employer = employer,
                TotalIncome = totalIncome
            };
        }

        public async Task<AutodecisionCompositeData> GetCompositeDataFromRedis(string key)
        {
            var cacheContent = await _redisCacheService.StringGetAsync(key);

            if (string.IsNullOrEmpty(cacheContent))
            {
                _logger.LogError($"Error getting content from Redis | Key: {key}");
                throw new Exception("Error getting content from Redis");
            }

            return JsonConvert.DeserializeObject<AutodecisionCompositeData>(cacheContent);
        }
    }
}
