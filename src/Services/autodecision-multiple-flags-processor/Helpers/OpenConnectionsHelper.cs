using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Contracts.ViewModels.OpenPayrollData;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Utility;

#pragma warning disable CS8603 // Possible null reference return
namespace AutodecisionMultipleFlagsProcessor.Helpers
{
    public static class OpenConnectionsHelper
    {
        public static bool IsFaceRecognitionPending(FaceRecognition faceRecognition)
        {
            if (faceRecognition.EnrollmentStatus is null || faceRecognition.FraudStatus is null)
                return true;

            return false;
        }

        public static bool IsFaceRecognitionDone(FaceRecognition faceRecognition) =>
            faceRecognition?.EnrollmentStatus?.ToUpper() == "DONE";


        public static bool HasDebitCardConnection(DebitCard debitCard) =>
            debitCard is not null && debitCard.IsConnected;

        public static bool HasOpenBankingConnection(OpenBanking openBanking) =>
            openBanking is not null && openBanking?.Connections?.Count > 0;

        public static bool IsDaysConnectedWithOpenBankingValid(int? daysConnectedWithOpenBanking)
        {
            const int maxConnectedDaysAccepted = 15;

            if (!daysConnectedWithOpenBanking.HasValue)
                return false;

            return (daysConnectedWithOpenBanking is >= 0 and <= maxConnectedDaysAccepted);
        }

        public static int? GetDaysFromLatestOpenBankingConnection(List<OpenBankingConnections> openBankingConnections)
        {
            if (openBankingConnections is null || !openBankingConnections.Any()) return null;
            var latestConnection = GetLatestOpenBankingConnection(openBankingConnections);
            return CalculateDaysDifference(latestConnection);
        }

        public static int? GetDaysFromLatestOpenPayrollConnection(List<OpenPayrollConnection> openPayrollConnections)
        {
            if (openPayrollConnections is null || !openPayrollConnections.Any()) return null;
            return openPayrollConnections.OrderByDescending(x => x.ConnectedAt).FirstOrDefault()?.ConnectedAt.GetDifferenceInDaysFromToday();
        }

        public static bool ShouldConnectOpenBanking(List<EmployerRulesItem> ruleGroup) =>
            ruleGroup.Any(r => r.Key == NewCreditPolicyRule.OpenBankingMandatory);

        public static bool ShouldConnectFaceId(List<EmployerRulesItem> ruleGroup) =>
            ruleGroup.Any(r => r.Key == NewCreditPolicyRule.FaceIdMandatory);

        public static bool ShouldConnectOpenPayroll(List<EmployerRulesItem> ruleGroup) =>
             ruleGroup.Any(r => r.Key == NewCreditPolicyRule.OpenPayrollMandatory);
        
        public static bool ShouldConnectDebitCard(Application application) =>
             application.FundingMethod == "debit_card" || application.PaymentType == "debit_card";

        public static bool MustConnectAtLeastOpenPayrollOrBanking(List<EmployerRulesItem> ruleGroup) =>
             ruleGroup.Any(r => r.Key == NewCreditPolicyRule.OpenPayrollOrBankingMandatory);

        private static OpenBankingConnections GetLatestOpenBankingConnection(List<OpenBankingConnections> openBankingConnections) =>
            openBankingConnections.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt).FirstOrDefault();

        private static int CalculateDaysDifference(OpenBankingConnections connection) =>
            connection.UpdatedAt.HasValue ?
            connection.UpdatedAt.Value.GetDifferenceInDaysFromToday() :
            connection.CreatedAt.GetDifferenceInDaysFromToday();
    }
}