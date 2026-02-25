using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Contracts.ViewModels.OpenPayrollData;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Utility;
using FlagValidatorHelperUtil = AutodecisionCore.Contracts.ViewModels.Helpers.FlagValidatorHelper;

namespace AutodecisionMultipleFlagsProcessor.Helpers
{
    public static class FlagValidatorHelper
    {
        public static bool CanValidateFlag(AutodecisionCompositeData compositeData, ProcessFlagResponseEvent? response = null)
        {
            response ??= new ProcessFlagResponseEvent();

            if (!compositeData.FlagValidatorHelper.EmployerAllowAutoDeny)
            {
                response.FlagResult = FlagResultEnum.Ignored;
                response.Message = "The application employer doesn't allow auto deny";
                return false;
            }

            if (!IsLFFOrLFAProgram(compositeData.Application.Program))
            {
                response.FlagResult = FlagResultEnum.Ignored;
                response.Message = $"The application program is {BMGMoneyProgram.GetProgramName(compositeData.Application.Program)}";
                return false;
            }

            if (!OpenPayrollIsValid(compositeData.OpenPayroll))
            {
                response.FlagResult = FlagResultEnum.Ignored;
                response.Message = "Open Payroll not connected";
                return false;
            }

            return true;
        }

        public static bool HasOpenPayrollInconsistency(AutodecisionCompositeData compositeData, ProcessFlagResponseEvent? response = null)
        {
            response ??= new ProcessFlagResponseEvent();

            if (IsValidSpecificHireDate(compositeData.Application))
            {
                response.FlagResult = FlagResultEnum.PendingApproval;
                response.Message += "Asking for customer the oldest paystub to manual approval ";
            }

            if (HireDateNullWithValidPaystub(compositeData))
            {
                response.FlagResult = FlagResultEnum.PendingApproval;
                response.Message += "The hire date is null but there is a valid paystub ";
            }

            if (HireDateIsValidTowardsOldestPayDate(compositeData))
            {
                response.FlagResult = FlagResultEnum.PendingApproval;
                response.Message += "The hire date is more recent than oldest pay date ";
            }

            return HasDefinedResponseMessage(response.Message);
        }

        public static bool HasDefinedResponseMessage(string message) => !string.IsNullOrEmpty(message);

        public static bool IsLFFOrLFAProgram(string program) =>
            program == BMGMoneyProgram.LoansForFeds || program == BMGMoneyProgram.LoansForAll;

        public static bool IsLAWProgram(string program) =>
          program == BMGMoneyProgram.LoansAtWork;

        public static bool OpenPayrollIsValid(OpenPayroll openPayroll) =>
             openPayroll.Connections.Count > 0 && openPayroll.Connections.Any(c => c.IsActive);

        public static bool HireDateNullWithValidPaystub(AutodecisionCompositeData autodecisionCompositeData)
        {
            if (!autodecisionCompositeData.Application.VerifiedDateOfHire.HasValue && OpenPayrollIsValid(autodecisionCompositeData.OpenPayroll))
                return true;

            return false;
        }

        public static bool HireDateIsValidTowardsOldestPayDate(AutodecisionCompositeData compositeData)
        {
            DateTime? oldestPayDate = FetchOldestPayDate(compositeData);

            if (oldestPayDate != null && oldestPayDate != DateTime.MinValue && compositeData.Application.VerifiedDateOfHire.HasValue)
                return !DateTimeUtil.IsBiggerOrEqual(oldestPayDate.Value, compositeData.Application.VerifiedDateOfHire.Value);

            return false;
        }

        private static DateTime? FetchOldestPayDate(AutodecisionCompositeData compositeData) =>
            OpenPayrollIsValid(compositeData.OpenPayroll) ? compositeData.OpenPayroll?.Connections?.FirstOrDefault()?.OldestPayDate.Date : null;

        public static bool IsValidSpecificHireDate(Application application)
        {
            if (application.EmployerId == 698 && IsDateBetween17And20MonthsAgo(application.VerifiedDateOfHire))
                return true;

            if (application.EmployerId == 915)
                return true;

            if (application.EmployerId == 2501 && application.VerifiedDateOfHire == new DateTime(2021, 11, 20).Date)
                return true;

            return false;
        }

        public static bool IsDateBetween17And20MonthsAgo(DateTime? verifiedDateOfHire)
        {
            if (!verifiedDateOfHire.HasValue)
                return false;

            DateTime today = DateTime.Today;
            DateTime seventeenMonthsAgo = today.AddMonths(-17).Date;
            DateTime twentyMonthsAgo = today.AddMonths(-20).Date;
            return verifiedDateOfHire.Value.Date >= twentyMonthsAgo && verifiedDateOfHire.Value.Date <= seventeenMonthsAgo;
        }


        #region Auto Deny Validator

        public static bool CanAutoDenyCustomer(AutodecisionCompositeData compositeData, ProcessFlagResponseEvent? response = null)
        {
            response ??= new ProcessFlagResponseEvent();

            if (!IsSubmissionTimeValidForDecline(compositeData.Application))
            {
                response.FlagResult = FlagResultEnum.Ignored;
                return false;
            }

            if (IsCustomerInSkipAutoDeny(compositeData.FlagValidatorHelper))
            {
                response.FlagResult = FlagResultEnum.Ignored;
                response.Message = "Customer can be skip";
                return false;
            }

            if (IsDebitCardMandatoryAndNotConnectionValid(compositeData))
            {
                response.FlagResult = FlagResultEnum.Ignored;
                response.Message = "Debit card mandatory and not connect";
                return false;
            }

            if (IsFaceIdMandatoryAndNotConnectionValid(compositeData))
            {
                response.FlagResult = FlagResultEnum.Ignored;
                response.Message = "FaceId is mandatory and not connect";
                return false;
            }

            if (IsOpenBankingMandatoryAndNotConnectionValid(compositeData))
            {
                response.FlagResult = FlagResultEnum.Ignored;
                response.Message = "Open Banking mandatory and not connected";
                return false;
            }

            if (HasPendingOrSubmittedDocuments(compositeData.FlagValidatorHelper))
            {
                response.FlagResult = FlagResultEnum.Ignored;
                response.Message = "Customer has pending documents";
                return false;
            }
            return true;
        }

        public static bool IsDebitCardMandatoryAndNotConnectionValid(AutodecisionCompositeData compositeData) =>
            OpenConnectionsHelper.ShouldConnectDebitCard(compositeData.Application) && !OpenConnectionsHelper.HasDebitCardConnection(compositeData.DebitCard);

        public static bool IsFaceIdMandatoryAndNotConnectionValid(AutodecisionCompositeData compositeData) =>
            OpenConnectionsHelper.ShouldConnectFaceId(compositeData.CreditPolicy.EmployerRules.EmployerRulesItems)
                && (OpenConnectionsHelper.IsFaceRecognitionPending(compositeData.FaceRecognition) || !OpenConnectionsHelper.IsFaceRecognitionDone(compositeData.FaceRecognition));

        public static bool IsOpenBankingMandatoryAndNotConnectionValid(AutodecisionCompositeData compositeData) =>
            OpenConnectionsHelper.ShouldConnectOpenBanking(compositeData.CreditPolicy.EmployerRules.EmployerRulesItems)
                && !OpenConnectionsHelper.HasOpenBankingConnection(compositeData.OpenBanking);

        public static bool IsSubmissionTimeValidForDecline(Application application)
        {
            const int minimumSubmissionTimeAllowedToDecline = 30;

            return application.SubmittedAt.GetDifferenceInMinutesFromNow() >= minimumSubmissionTimeAllowedToDecline;
        }

        public static bool IsCustomerInSkipAutoDeny(FlagValidatorHelperUtil util)
        {
            const int limitDaysUntilIgnoreAutoDeny = 15;

            if (util == null || util.CustomerSkipAutoDeny == null || !util.CustomerSkipAutoDeny.Active)
                return false;

            return util.CustomerSkipAutoDeny.ActivatedAt.GetDifferenceInDaysFromToday() < limitDaysUntilIgnoreAutoDeny;
        }

        public static bool HasPendingOrSubmittedDocuments(FlagValidatorHelperUtil util)
        {
            if (util.ApplicationDocuments.Count > 0)
            {
                if (util.ApplicationDocuments.Count == 1 && util.ApplicationDocuments.Any(x => x.DocumentType == DocumentType.eConsent))
                    return false;

                if (util.ApplicationDocuments.Any(x => !x.Uploaded))
                    return true;
            }
            return false;
        }

        #endregion
    }
}