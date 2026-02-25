using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.DTOs;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using BmgMoney.FeatureToggle.DotNetCoreClient.Client;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class TUBankruptcyHandlerTests
    {
        private readonly Mock<ILogger<TUBankruptcyHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;
        private readonly Mock<ICreditInquiry> _mockCreditInquiry;
        private readonly Mock<ICustomerInfo> _mockCustomerInfo;
        private readonly Mock<IFeatureToggleClient> _mockFeatureToggle;

        public TUBankruptcyHandlerTests()
        {
            _mockLogger = new Mock<ILogger<TUBankruptcyHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();
            _mockCreditInquiry = new Mock<ICreditInquiry>();
            _mockCustomerInfo = new Mock<ICustomerInfo>();
            _mockFeatureToggle = new Mock<IFeatureToggleClient>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
            FlagHelperMockUtility.MockFeatureToggleInstance(_mockFeatureToggle, false);
        }

        [Fact]
        public void WhenSuccessPacerAndBankruptcyPacerIsTrueAndPacerValidationResultsHasTrueOnBothPropertiesShouldApproveFlag()
        {
            var pacerValidation = new PacerValidationDto()
            {
                CheckApproveFlag = true,
                CheckDeclineApp = false,
            };
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application
                {
                    Id = 1,
                    LoanNumber = "0001"
                },
                Customer = new Customer
                {
                    Id = 1,
                    FirstName = "test",
                    LastName = "test",
                    DateOfBirth = new DateTime(1990, 4, 15),
                    StreetAddress = "test",
                    CityName = "test",
                    StateName = "test",
                    ZipCode = "02020",
                    Ssn = "1511"
                },
                TransunionResult = new TransunionResult
                {
                    FirstName = "test",
                    LastName = "test",
                    BirthDate = new DateTime(1990, 4, 15),
                    StreetAddress = "test",
                    CityName = "test",
                    StateName = "test",
                    ZipCode = "02020",
                    SSN = "1511"
                }
            };

            var handler = new TUBankruptcyHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCreditInquiry.Object, _mockCustomerInfo.Object, _mockFeatureToggle.Object);


            _mockFeatureToggle.Setup(x => x.IsDisabled("check_bk_pacer_first")).Returns(false);
            _mockFeatureToggle.Setup(x => x.IsEnabled("autodecision_run_pacer")).Returns(true);
            _mockCreditInquiry.Setup(x => x.CheckPacerBankruptcy("0001")).Returns("{\"success\": true, \"bankruptcy\" : true}");
            _mockCustomerInfo.Setup(x => x.ValidateBankruptcy(1)).Returns(pacerValidation);
            var resposta = handler.ProcessFlag(obj);

            Assert.Equal(AutodecisionCore.Contracts.Enums.FlagResultEnum.Approved, resposta.FlagResult);



        }

        [Fact]
        public void WhenHasPacerAndTransunionBankruptcyAndPacerValidationResultsHasTrueOnBothPropertiesShouldApproveFlag()
        {
            var pacerValidation = new PacerValidationDto()
            {
                CheckApproveFlag = true,
                CheckDeclineApp = false,
            };
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application
                {
                    Id = 1,
                    LoanNumber = "0001"
                },
                Customer = new Customer
                {
                    Id = 1,
                    FirstName = "test",
                    LastName = "test",
                    DateOfBirth = new DateTime(1990, 4, 15),
                    StreetAddress = "test",
                    CityName = "test",
                    StateName = "test",
                    ZipCode = "02020",
                    Ssn = "1511"
                },
                TransunionResult = new TransunionResult
                {
                    FirstName = "test",
                    LastName = "test",
                    BirthDate = new DateTime(1990, 4, 15),
                    StreetAddress = "test",
                    CityName = "test",
                    StateName = "test",
                    ZipCode = "02020",
                    SSN = "1511"
                }
            };

            var handler = new TUBankruptcyHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCreditInquiry.Object, _mockCustomerInfo.Object, _mockFeatureToggle.Object);


            _mockFeatureToggle.Setup(x => x.IsDisabled("check_bk_pacer_first")).Returns(true);
            _mockFeatureToggle.Setup(x => x.IsEnabled("autodecision_run_pacer")).Returns(true);
            _mockFeatureToggle.Setup(x => x.IsEnabled("autodecision_run_transunion")).Returns(true);
            _mockCreditInquiry.Setup(x => x.CheckPacerBankruptcy("0001")).Returns("{\"success\": true, \"bankruptcy\" : true}");
            _mockCreditInquiry.Setup(x => x.CheckTransunionBankruptcy("0001")).Returns("{\"success\": true, \"bankruptcy\" : true}");
            _mockCustomerInfo.Setup(x => x.ValidateBankruptcy(1)).Returns(pacerValidation);
            var resposta = handler.ProcessFlag(obj);

            Assert.Equal(AutodecisionCore.Contracts.Enums.FlagResultEnum.Approved, resposta.FlagResult);
        }

        [Fact]
        public void WhenHasPacerAndTransunionBankruptcyAndPacerValidationResultsHasfalseInOnePropertyShouldRaiseFlag()
        {
            var pacerValidation = new PacerValidationDto()
            {
                CheckApproveFlag = false,
                CheckDeclineApp = true,
            };
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application
                {
                    Id = 1,
                    LoanNumber = "0001"
                },
                Customer = new Customer
                {
                    Id = 1,
                    FirstName = "test",
                    LastName = "test",
                    DateOfBirth = new DateTime(1990, 4, 15),
                    StreetAddress = "test",
                    CityName = "test",
                    StateName = "test",
                    ZipCode = "02020",
                    Ssn = "1511"
                },
                TransunionResult = new TransunionResult
                {
                    FirstName = "test",
                    LastName = "test",
                    BirthDate = new DateTime(1990, 4, 15),
                    StreetAddress = "test",
                    CityName = "test",
                    StateName = "test",
                    ZipCode = "02020",
                    SSN = "1511"
                }
            };

            var handler = new TUBankruptcyHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCreditInquiry.Object, _mockCustomerInfo.Object, _mockFeatureToggle.Object);


            _mockFeatureToggle.Setup(x => x.IsDisabled("check_bk_pacer_first")).Returns(true);
            _mockFeatureToggle.Setup(x => x.IsEnabled("autodecision_run_pacer")).Returns(true);
            _mockFeatureToggle.Setup(x => x.IsEnabled("autodecision_run_transunion")).Returns(true);
            _mockCreditInquiry.Setup(x => x.CheckPacerBankruptcy("0001")).Returns("{\"success\": true, \"bankruptcy\" : true}");
            _mockCreditInquiry.Setup(x => x.CheckTransunionBankruptcy("0001")).Returns("{\"success\": true, \"bankruptcy\" : true}");
            _mockCustomerInfo.Setup(x => x.ValidateBankruptcy(1)).Returns(pacerValidation);
            var resposta = handler.ProcessFlag(obj);

            Assert.Equal(AutodecisionCore.Contracts.Enums.FlagResultEnum.PendingApproval, resposta.FlagResult);

        }

        [Fact]
        public void WhenHasOnlyTransunionBankruptcyShouldRaiseFlag()
        {

            var obj = new AutodecisionCompositeData()
            {
                Application = new Application
                {
                    Id = 1,
                    LoanNumber = "0001"
                },
                Customer = new Customer
                {
                    Id = 1,
                    FirstName = "test",
                    LastName = "test",
                    DateOfBirth = new DateTime(1990, 4, 15),
                    StreetAddress = "test",
                    CityName = "test",
                    StateName = "test",
                    ZipCode = "02020",
                    Ssn = "1511"
                },
                TransunionResult = new TransunionResult
                {
                    FirstName = "test",
                    LastName = "test",
                    BirthDate = new DateTime(1990, 4, 15),
                    StreetAddress = "test",
                    CityName = "test",
                    StateName = "test",
                    ZipCode = "02020",
                    SSN = "1511"
                }
            };

            var handler = new TUBankruptcyHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCreditInquiry.Object, _mockCustomerInfo.Object, _mockFeatureToggle.Object);


            _mockFeatureToggle.Setup(x => x.IsDisabled("check_bk_pacer_first")).Returns(true);
            _mockFeatureToggle.Setup(x => x.IsEnabled("autodecision_run_transunion")).Returns(true);
            _mockCreditInquiry.Setup(x => x.CheckTransunionBankruptcy("0001")).Returns("{\"success\": true, \"bankruptcy\" : true}");
            var resposta = handler.ProcessFlag(obj);

            Assert.Equal(AutodecisionCore.Contracts.Enums.FlagResultEnum.PendingApproval, resposta.FlagResult);

        }
        [Fact]
        public void WhenCanNotCallPacerAndTrasunionShouldRaiseFlag()
        {

            var obj = new AutodecisionCompositeData()
            {
                Application = new Application
                {
                    Id = 1,
                    LoanNumber = "0001"
                },
                Customer = new Customer
                {
                    Id = 1,
                    FirstName = "test",
                    LastName = "test",
                    DateOfBirth = new DateTime(1990, 4, 15),
                    StreetAddress = "test",
                    CityName = "test",
                    StateName = "test",
                    ZipCode = "02020",
                    Ssn = "1511"
                },
                TransunionResult = new TransunionResult
                {
                    FirstName = "test",
                    LastName = "test",
                    BirthDate = new DateTime(1990, 4, 15),
                    StreetAddress = "test",
                    CityName = "test",
                    StateName = "test",
                    ZipCode = "02020",
                    SSN = "1511"
                }
            };

            var handler = new TUBankruptcyHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCreditInquiry.Object, _mockCustomerInfo.Object, _mockFeatureToggle.Object);

            var resposta = handler.ProcessFlag(obj);

            Assert.Equal(AutodecisionCore.Contracts.Enums.FlagResultEnum.PendingApproval, resposta.FlagResult);

        }
    }
}
