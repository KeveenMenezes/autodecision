using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Data.Repositories.Interfaces;
using AutodecisionMultipleFlagsProcessor.DTOs;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class HouseholdHitHandlerTests
    {
        private readonly Mock<ILogger<HouseholdHitHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;
        private readonly Mock<IHouseholdHitRepository> _mockHouseholdHitRepository;
        private readonly Mock<ICustomerInfo> _mockCustomerInfo;

        public HouseholdHitHandlerTests()
        {
            _mockLogger = new Mock<ILogger<HouseholdHitHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();
            _mockHouseholdHitRepository = new Mock<IHouseholdHitRepository>();
            _mockCustomerInfo = new Mock<ICustomerInfo>();
            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public async void WhenReturnSimilarAddressShouldRaiseFlag()
        {

            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "000002"
                },
                Customer = new Customer()
                {
                    Id = 32784,
                    StreetAddress = "444 Brickell Avenue",
                    ZipCode = "33150",
                    CityName = "MIAMI",
                    StateAbbreviation = "FL",
                    UnitNumber = "250"
                },

            };
            SimilarAddressDto similarAddress = new SimilarAddressDto()
            {
                LoanNumber = "000001",
                StreetAddress = "444 Brickell Avenue",
                Zipcode = "33150",
                CityName = "MIAMI",
                StateAbbreviation = "FL",
                CustomerId = 32801,
                UnitNumber = "250"
            };
            List<SimilarAddressDto> similarAddressList = new List<SimilarAddressDto>() { similarAddress };

            var handler = new HouseholdHitHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockHouseholdHitRepository.Object, _mockCustomerInfo.Object);

            _mockHouseholdHitRepository.Setup(x => x.GetSimilarAddressListAsync(obj.Customer, 0.94m)).ReturnsAsync(similarAddressList);
            var resposta = await handler.ProcessFlag(obj);

            Assert.Equal(AutodecisionCore.Contracts.Enums.FlagResultEnum.PendingApproval, resposta.FlagResult);
        }

        [Fact]
        public async void WhenReturnSimilarAddressAndStateAbbeviationIsntFLAndApplicationTypeIsRefiShouldReturnProccessed()
        {

            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "000002",
                    Type = ApplicationType.Refi,
                    StateAbbreviation = "CA"
                },
                Customer = new Customer()
                {
                    Id = 32784,
                    StreetAddress = "444 Brickell Avenue",
                    ZipCode = "33150",
                    CityName = "MIAMI",
                    StateAbbreviation = "FL",
                    UnitNumber = "250"
                },

            };
            SimilarAddressDto similarAddress = new SimilarAddressDto()
            {
                LoanNumber = "000001",
                StreetAddress = "444 Brickell Avenue",
                Zipcode = "33150",
                CityName = "MIAMI",
                StateAbbreviation = "FL",
                CustomerId = 32801,
                UnitNumber = "250"
            };
            List<SimilarAddressDto> similarAddressList = new List<SimilarAddressDto>() { similarAddress };


            var handler = new HouseholdHitHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockHouseholdHitRepository.Object, _mockCustomerInfo.Object);

            _mockHouseholdHitRepository.Setup(x => x.GetSimilarAddressListAsync(obj.Customer, 0.94m)).ReturnsAsync(similarAddressList);
            var resposta = await handler.ProcessFlag(obj);

            Assert.Equal(AutodecisionCore.Contracts.Enums.FlagResultEnum.Processed, resposta.FlagResult);
        }

        [Fact]
        public async void WhenReturnSimilarAddressAndApplicationTypeIsRefiAndCheckHasBookReturnFalseShouldReturnProccessed()
        {

            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "000002",
                    Type = ApplicationType.Refi,
                    StateAbbreviation = "FL"
                },
                Customer = new Customer()
                {
                    Id = 32784,
                    StreetAddress = "444 Brickell Avenue",
                    ZipCode = "33150",
                    CityName = "MIAMI",
                    StateAbbreviation = "FL",
                    UnitNumber = "250"
                },

            };
            SimilarAddressDto similarAddress = new SimilarAddressDto()
            {
                LoanNumber = "000001",
                StreetAddress = "444 Brickell Avenue",
                Zipcode = "33150",
                CityName = "MIAMI",
                StateAbbreviation = "FL",
                CustomerId = 32801,
                UnitNumber = "250"
            };
            List<SimilarAddressDto> similarAddressList = new List<SimilarAddressDto>() { similarAddress };


            var handler = new HouseholdHitHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockHouseholdHitRepository.Object, _mockCustomerInfo.Object);

            _mockHouseholdHitRepository.Setup(x => x.GetSimilarAddressListAsync(obj.Customer, 0.94m)).ReturnsAsync(similarAddressList);
            _mockCustomerInfo.Setup(x => x.CheckHasBook(similarAddressList)).ReturnsAsync(false);

            var resposta = await handler.ProcessFlag(obj);

            Assert.Equal(AutodecisionCore.Contracts.Enums.FlagResultEnum.Processed, resposta.FlagResult);
        }

        [Fact]
        public async void WhenReturnOneSimilarAddressAndIsWhiteListRelatedReturnTrueShouldReturnProccessed()
        {

            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "000002",
                    Type = ApplicationType.Refi,
                    StateAbbreviation = "FL"
                },
                Customer = new Customer()
                {
                    Id = 32784,
                    StreetAddress = "444 Brickell Avenue",
                    ZipCode = "33150",
                    CityName = "MIAMI",
                    StateAbbreviation = "FL",
                    UnitNumber = "250"
                },

            };
            SimilarAddressDto similarAddress = new SimilarAddressDto()
            {
                LoanNumber = "000001",
                StreetAddress = "444 Brickell Avenue",
                Zipcode = "33150",
                CityName = "MIAMI",
                StateAbbreviation = "FL",
                CustomerId = 32801,
                UnitNumber = "250"
            };
            List<SimilarAddressDto> similarAddressList = new List<SimilarAddressDto>() { similarAddress };


            var handler = new HouseholdHitHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockHouseholdHitRepository.Object, _mockCustomerInfo.Object);

            _mockHouseholdHitRepository.Setup(x => x.GetSimilarAddressListAsync(obj.Customer, 0.94m)).ReturnsAsync(similarAddressList);
            _mockCustomerInfo.Setup(x => x.CheckHasBook(similarAddressList)).ReturnsAsync(true);
            _mockCustomerInfo.Setup(x => x.IsWhitelistRelated(obj.Customer.Id, similarAddressList.FirstOrDefault().CustomerId)).ReturnsAsync(true);

            var resposta = await handler.ProcessFlag(obj);

            Assert.Equal(AutodecisionCore.Contracts.Enums.FlagResultEnum.Processed, resposta.FlagResult);
        }

        [Fact]
        public async void WhenDoesntReturnSimilarAddressReturnProccessed()
        {

            var obj = new AutodecisionCompositeData()
            {
                Application = new Application()
                {
                    LoanNumber = "000002",
                    Type = ApplicationType.Refi,
                    StateAbbreviation = "FL"
                },
                Customer = new Customer()
                {
                    Id = 32784,
                    StreetAddress = "444 Brickell Avenue",
                    ZipCode = "33150",
                    CityName = "MIAMI",
                    StateAbbreviation = "FL",
                    UnitNumber = "250"
                },

            };
            var handler = new HouseholdHitHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockHouseholdHitRepository.Object, _mockCustomerInfo.Object);
            _mockHouseholdHitRepository.Setup(x => x.GetSimilarAddressListAsync(obj.Customer, 0.94m)).ReturnsAsync(new List<SimilarAddressDto>());

            var resposta = await handler.ProcessFlag(obj);

            Assert.Equal(AutodecisionCore.Contracts.Enums.FlagResultEnum.Processed, resposta.FlagResult);
        }
    }
}