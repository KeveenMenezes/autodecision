using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Contracts.ViewModels.Helpers;
using AutodecisionCore.Contracts.ViewModels.OpenPayrollData;
using AutodecisionCore.Data.Models;

namespace AutodecisionCore.Core.AutoApprovalCore.DTO
{
    public class AutoApprovalRequest
    {
        public string LoanNumber { get; set; }
        public string Reason { get; set; }
        public List<AutoApprovalFundingMethod> AutoApprovalFundingMethods { get; set; }
        public List<AutoApprovalPaymentType> AutoApprovalPaymentTypes { get; set; }
        public List<AutoApprovalUwCluster> AutoApprovalUwClusters { get; set; }
        public OpenPayroll OpenPayroll { get; set; }
        public OpenBanking OpenBanking { get; set; }
        public Application Application { get; set; }
        public List<LastApplication> LastApplications { get; set; }
        public CreditPolicy CreditPolicy { get; set; }
        public Customer Customer { get; set; }
        public DebitCard DebitCard { get; set; }
        public FaceRecognition FaceRecognition { get; set; }
        public bool? AllotmentRequested { get; set; }
        public CreditRisk CreditRisk { get; set; }
        public Employer Employer { get; set; }
        public List<ApplicationDocuments> ApplicationDocuments { get; set; }
    }
}
