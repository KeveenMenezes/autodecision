using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Core.AutoApprovalCore.DTO;
using AutodecisionCore.Data.Models;
using AutodecisionCore.Extensions;
using AutodecisionCore.Utils;

namespace AutodecisionCore.Tests.AutoApproval.Rules
{
    public static class AutoApprovalTestUtils
    {
        public static AutoApprovalRequest CreateSetup()
        {
            var request = new AutoApprovalRequest();

            request.AutoApprovalFundingMethods = new List<AutoApprovalFundingMethod>();
            request.AutoApprovalUwClusters = new List<AutoApprovalUwCluster>();
            request.AutoApprovalPaymentTypes = new List<AutoApprovalPaymentType>();

            request.AutoApprovalPaymentTypes.Add(new AutoApprovalPaymentType()
            {
                CreatedAt = DateTime.Now,
                PaymentType = "allotment",
                Id = 1,
                IsAllowed = true,
                IsDeleted = false
            });

            request.AutoApprovalPaymentTypes.Add(new AutoApprovalPaymentType()
            {
                CreatedAt = DateTime.Now,
                PaymentType = "split_direct_deposit",
                Id = 2,
                IsAllowed = true,
                IsDeleted = false
            });

            request.AutoApprovalPaymentTypes.Add(new AutoApprovalPaymentType()
            {
                CreatedAt = DateTime.Now,
                PaymentType = "debit_card",
                Id = 3,
                IsAllowed = true,
                IsDeleted = false
            });

            request.AutoApprovalPaymentTypes.Add(new AutoApprovalPaymentType()
            {
                CreatedAt = DateTime.Now,
                PaymentType = "ach",
                Id = 4,
                IsAllowed = false,
                IsDeleted = false
            });

            request.AutoApprovalUwClusters.Add(new AutoApprovalUwCluster()
            {
                Id = 1,
                UwCluster = "A",
                IsAllowed = true
            });

            request.AutoApprovalUwClusters.Add(new AutoApprovalUwCluster()
            {
                Id = 1,
                UwCluster = "B",
                IsAllowed = true
            });

            request.AutoApprovalUwClusters.Add(new AutoApprovalUwCluster()
            {
                Id = 1,
                UwCluster = "C",
                IsAllowed = true
            });

            request.AutoApprovalUwClusters.Add(new AutoApprovalUwCluster()
            {
                Id = 1,
                UwCluster = "D",
                IsAllowed = false
            });

            request.AutoApprovalUwClusters.Add(new AutoApprovalUwCluster()
            {
                Id = 1,
                UwCluster = "E",
                IsAllowed = false
            });

            request.AutoApprovalFundingMethods.Add(new AutoApprovalFundingMethod()
            {
                CreatedAt = DateTime.Now,
                FundingMethod = "ach",
                Id = 1,
                IsAllowed = false,
                IsDeleted = false
            });

            request.AutoApprovalFundingMethods.Add(new AutoApprovalFundingMethod()
            {
                CreatedAt = DateTime.Now,
                FundingMethod = "debit_card",
                Id = 2,
                IsAllowed = true,
                IsDeleted = false
            });

            request.CreditPolicy = new CreditPolicy()
            {
                EmployerRules = new EmployerRules()
                {
                    EmployerRulesItems = new List<EmployerRulesItem>()
                }
            };

            request.CreditPolicy.EmployerRules.EmployerRulesItems.Add(new EmployerRulesItem()
            {
                Key = "commitment_level",
                Max = "0.10"
            });

            request.CreditPolicy.EmployerRules.EmployerRulesItems.Add(new EmployerRulesItem()
            {
                Key = "length_of_employment",
                Min = "365"
            });


            return request;
        }

        public static Application CreateApplication(int employerId, string program, string fundingMethod = ApplicationFundingMethod.ACH) =>
            new Application()
            {
                LoanNumber = "123456789",
                Type = ApplicationType.NewLoan,
                Program = program,
                ProductId = ApplicationProductId.Standard,
                FundingMethod = fundingMethod,
                PaymentType = PayrollType.Allotment,
                UwCluster = "A",
                AmountOfPayment = 115m,
                EmployerId = employerId,
                CreatedBy = "agent"
            };


        public static Application CreateCashlessApplicationWithDebitCardFunding(int employerId, string program, string createdBy = "agent") =>
            new Application()
            {
                LoanNumber = "123456789",
                Type = ApplicationType.Refi,
                Program = program,
                ProductId = ApplicationProductId.Cashless,
                FundingMethod = ApplicationFundingMethod.DebitCard,
                PaymentType = PayrollType.ACH,
                UwCluster = "A",
                AmountOfPayment = 115m,
                EmployerId = employerId,
                CreatedBy = createdBy
            };

        public static LastApplication CreateLastApplicationAsNewLoanBooked(string program) =>
            new LastApplication()
            {
                Id = 1,
                LoanNumber = "123456789",
                Type = ApplicationType.NewLoan,
                Status = ApplicationStatus.Booked,
                CreatedAt = DateTimeUtil.Now,
                AmountOfPayment = 115m,
                Program = program
            };

        public static DebitCard CreateDebitCardConnection(string vendor) =>
            new DebitCard()
            {
                CustomerId = 12345,
                Active = true,
                CardNumber = "1234",
                CardBrand = "VISA",
                IsConnected = true,
                Expiration = "09/29",
                Vendor = vendor
            };

        public static void SetApplicationAndLastApplicationsWithDifferentAmounts(this AutoApprovalRequest request)
        {
            var application = MockApplication();
            var lastApplications = new List<LastApplication>()
            {
                MockLastApplication(amount: 100, ApplicationStatus.Liquidated),
                MockLastApplication(amount: 170, ApplicationStatus.Liquidated),
            };

            request.LoanNumber = "123456789";
            request.Application = application;
            request.LastApplications = lastApplications;
        }

        public static void SetApplicationAndLastApplicationsWithDifferentReconciliationSystems(this AutoApprovalRequest request)
        {
            var application = MockApplication("allotment", amount: 150);
            var lastApplications = new List<LastApplication>()
            {
                MockLastApplication(amount: 100, ApplicationStatus.Liquidated),
                MockLastApplication(amount: 150, ApplicationStatus.Booked, "firstnet")
            };

            request.LoanNumber = "123456789";
            request.Application = application;
            request.LastApplications = lastApplications;
        }

        public static void SetApplicationAndLastApplicationsWithSameAllotmentInformation(this AutoApprovalRequest request)
        {
            var application = MockApplication("allotment", amount: 150);
            var lastApplications = new List<LastApplication>()
            {
                MockLastApplication(amount: 100, ApplicationStatus.Liquidated),
                MockLastApplication(amount: 150, ApplicationStatus.Booked)
            };

            request.LoanNumber = "123456789";
            request.Application = application;
            request.LastApplications = lastApplications;
        }

        public static Application MockApplication(string paymentType = "allotment", decimal amount = 150,
            string type = ApplicationType.Refi, string reconciliationSystem = "jpmorgan",
            string loanNumber = "123456789", string program = ApplicationProgram.LoansForAll) =>
            new Application()
            {
                LoanNumber = loanNumber,
                PaymentType = paymentType,
                AmountOfPayment = amount,
                BankAccountNumber = "777777",
                BankRoutingNumber = "123456789",
                Type = type,
                ReconciliationSystem = reconciliationSystem,
                Program = program
            };

        public static LastApplication MockLastApplication(decimal amount = 150, string status = ApplicationStatus.Booked,
            string reconciliationSystem = "jpmorgan", string type = ApplicationType.Refi,
            string loanNumber = "123456789", string program = ApplicationProgram.LoansForAll) =>
            new LastApplication()
            {
                LoanNumber = loanNumber,
                AmountOfPayment = amount,
                BankAccountNumber = "777777",
                BankRoutingNumber = "123456789",
                Status = status,
                ReconciliationSystem = reconciliationSystem,
                Type = type,
                Program = program
            };
    }
}