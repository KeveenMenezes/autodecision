using AutodecisionCore.Data.Models.Base;
using Google.Protobuf.WellKnownTypes;

namespace AutodecisionCore.Data.Models
{
    public class Allotment : BaseModel
    {
        public string LoanNumber { get; set; }
        public string ReconciliationSystem { get; set; }
        public string RoutingNumber { get; set; }
        public string AccountNumber { get; set; }
        public decimal Value { get; set; }

        public Allotment(string loanNumber, string reconciliationSystem, string routingNumber, string accountNumber, decimal value)
        {
            LoanNumber = loanNumber;
            ReconciliationSystem = reconciliationSystem;
            RoutingNumber = routingNumber;
            AccountNumber = accountNumber;
            Value = value;
        }

        public void ChangeAllotmentInfo(string reconciliationSystem, string routingNumber, string accountNumber, decimal value)
        {
            ReconciliationSystem = reconciliationSystem;
            RoutingNumber = routingNumber;
            AccountNumber = accountNumber;
            Value = value;
        }
    }
}
