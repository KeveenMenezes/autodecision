using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Contracts.ViewModels.Helpers;
using AutodecisionCore.Extensions;
using AutodecisionCore.Services.Interfaces;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;

namespace AutodecisionCore.Services
{
    public class CreditRiskService : ICreditRiskService
    {
        private readonly ILogger<CreditRiskService> _logger;
        private readonly IFeatureToggleClient _featureToggleClient;

        public CreditRiskService(ILogger<CreditRiskService> logger, IFeatureToggleClient featureToggleClient)
        {
            _logger = logger;
            _featureToggleClient = featureToggleClient;
        }

        public bool ValidateAllowedLoanToCalculateScore(Application application, Employer employer)
        {
            _logger.LogInformation("Start - ValidateAllowedLoanToCalculateScore {0}", application.LoanNumber);

            List<string> allowedPrograms = new List<string>()
            {
                BMGMoneyProgram.LoansForFeds,
                BMGMoneyProgram.LoansForAll
            };

            List<int?> allowedEmployerSubPrograms = new List<int?>()
            {
                BMGMoneySubProgram.LoansForAll,
                BMGMoneySubProgram.LoansForFeds,
                BMGMoneySubProgram.LoansForAllHighTurnOver,
                BMGMoneySubProgram.LoansForAllMediumTurnOver,
                BMGMoneySubProgram.AlternativeLoans
            };

            _logger.LogInformation("Params to validate {0}: AppProgram:{1} | AppType: {2} | EmpSubProgram:{3}", application.LoanNumber, application.Program, application.Type, employer.SubProgramId);

            if (!allowedPrograms.Contains(application.Program))
            {
                return false;
            }

            if (!allowedEmployerSubPrograms.Contains(employer.SubProgramId))
            {
                return false;
            }

            if (!_featureToggleClient.IsEnabled("system_risk_score_refi_validation") && application.Type == ApplicationType.Refi)
            {
                return false;
            }

            _logger.LogInformation("ValidateAllowedLoanToCalculateScore Finished - {0} : Invalid", application.LoanNumber);
            return true;
        }

        public bool ValidateDocAllotmentSDD(Application application, Employer employer, List<ApplicationDocuments>? applicationDocuments = null)
        {
            _logger.LogInformation("Start - ValidateDocAllotmentSDD {0}", application.LoanNumber);

            List<string> programs = new List<string>()
            {
                BMGMoneyProgram.LoansForFeds
            };

            List<int?> employerSubPrograms = new List<int?>()
            {
                BMGMoneySubProgram.LoansForFeds
            };

            var allotmentOrSdd = applicationDocuments != null && applicationDocuments.Any(a => a.DocumentType == DocumentType.AllotmentProof || a.DocumentType == DocumentType.SplitDirectDepositProof);

            if (programs.Contains(application.Program) && employerSubPrograms.Contains(employer.SubProgramId) && allotmentOrSdd)
            {
                _logger.LogInformation("ValidateDocAllotmentSDD Finished - {0} : Contains", application.LoanNumber);
                return true;
            }

            _logger.LogInformation("ValidateDocAllotmentSDD Finished - {0} : Not Contain", application.LoanNumber);
            return false;
        }
    }
}
