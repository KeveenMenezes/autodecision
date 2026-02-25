using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.HttpRepositories;
using AutodecisionCore.Data.HttpRepositories.DTOs;
using AutodecisionCore.Extensions;
using BMGMoney.SDK.V2.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace AutodecisionCore.Tests.HttpRepositories
{
    public class ApplicationRepositoryTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IHttpService> _mockHttpService;
        private readonly Mock<ILogger<ApplicationRepository>> _mockLogger;

        public ApplicationRepositoryTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockHttpService = new Mock<IHttpService>();
            _mockLogger = new Mock<ILogger<ApplicationRepository>>();
        }

        [Fact]
        public async Task GetApplicationInfoReturnsApplicationWhenHttpCallSucceeds()
        {
            // Arrange
            var loanNumber = "12345678";
            var mockApplicationDto = new ApplicationDTO
            {
                LoanNumber = "12345678",
                CustomerId = 12345,
                AmountOfPayment = 170,
                BankRoutingNumber = "55555",
                BankAccountNumber = "987654321",
                SubmittedAt = DateTime.Now,
                EmployerId = 1,
                StateAbbreviation = "FL",
                BrowserFingerprint = "abcde1fghij2klmno3pqrst4uwvx5yz6",
                EmployerKey = "bmg_money",
                FundingMethod = "debit_card",
                Id = 123,
                LoanAmount = 2000,
                LoanTermsStateAbbreviation = "FL",
                PaymentType = "debit_card",
                PhoneNumber = "1 12345 6789",
                ProductId = 1,
                ProductKey = "standard",
                Program = "1",
                StateIpUserRequest = "192.168.0.0",
                Status = ApplicationStatus.Processing,
                Type = ApplicationType.NewLoan,
                UwCluster = "B",
                ReconciliationSystem = "firstnetpassthru",
                HighRisk = false,
                VerifiedDateOfHire = DateTime.Now.AddDays(1000),
            };

            _mockConfiguration.Setup(c => c["Apis:CustomerInfoApi"]).Returns("http://test.api");

            var httpResponse = (
                Success: true,
                StatusCode: HttpStatusCode.OK,
                Response: mockApplicationDto);

            _mockHttpService.Setup(h =>
                    h.GetAsync<ApplicationDTO>(
                        It.Is<string>(url => url == "http://test.api/application/autodecision-info/" + loanNumber),
                        It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(httpResponse);

            var repository = new ApplicationRepository(_mockConfiguration.Object, _mockHttpService.Object, _mockLogger.Object);

            // Act
            var result = await repository.GetApplicationInfo(loanNumber);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Application>(result);
            Assert.Equal(loanNumber, result.LoanNumber);
            Assert.Equal(httpResponse.Response.CustomerId, result.CustomerId);
            Assert.Equal(httpResponse.Response.AmountOfPayment, result.AmountOfPayment);
            Assert.Equal(httpResponse.Response.BankRoutingNumber, result.BankRoutingNumber);
            Assert.Equal(httpResponse.Response.BankAccountNumber, result.BankAccountNumber);
            Assert.Equal(httpResponse.Response.SubmittedAt, result.SubmittedAt);
            Assert.Equal(httpResponse.Response.EmployerId, result.EmployerId);
            Assert.Equal(httpResponse.Response.StateAbbreviation, result.StateAbbreviation);
            Assert.Equal(httpResponse.Response.BrowserFingerprint, result.BrowserFingerprint);
            Assert.Equal(httpResponse.Response.EmployerKey, result.EmployerKey);
            Assert.Equal(httpResponse.Response.FundingMethod, result.FundingMethod);
            Assert.Equal(httpResponse.Response.Id, result.Id);
            Assert.Equal(httpResponse.Response.LoanAmount, result.LoanAmount);
            Assert.Equal(httpResponse.Response.LoanTermsStateAbbreviation, result.LoanTermsStateAbbreviation);
            Assert.Equal(httpResponse.Response.PaymentType, result.PaymentType);
            Assert.Equal(httpResponse.Response.PhoneNumber, result.PhoneNumber);
            Assert.Equal(httpResponse.Response.ProductId, result.ProductId);
            Assert.Equal(httpResponse.Response.ProductKey, result.ProductKey);
            Assert.Equal(httpResponse.Response.Program, result.Program);
            Assert.Equal(httpResponse.Response.StateIpUserRequest, result.StateIpUserRequest);
            Assert.Equal(httpResponse.Response.Status, result.Status);
            Assert.Equal(httpResponse.Response.Type, result.Type);
            Assert.Equal(httpResponse.Response.UwCluster, result.UwCluster);
            Assert.Equal(httpResponse.Response.ReconciliationSystem, result.ReconciliationSystem);
            //TODO - UPDATE CONTRACT NUGGET VERSION TO >1.0.27 TO ACCESS HIGH RISK FIELD
            //Assert.Equal(httpResponse.Response.HighRisk, result.HighRisk);

            // Assert.True(condition)
            Assert.True(result.SubmittedAt > DateTime.MinValue);

            // Assert.NotEmpty(string)
            Assert.NotEmpty(result.LoanNumber);
            Assert.NotEmpty(result.BankRoutingNumber);
            Assert.NotEmpty(result.BankAccountNumber);
            Assert.NotEmpty(result.StateAbbreviation);
            Assert.NotEmpty(result.BrowserFingerprint);
            Assert.NotEmpty(result.EmployerKey);
            Assert.NotEmpty(result.FundingMethod);
            Assert.NotEmpty(result.PaymentType);
            Assert.NotEmpty(result.PhoneNumber);
            Assert.NotEmpty(result.ProductKey);
            Assert.NotEmpty(result.StateIpUserRequest);

            _mockHttpService.Verify(h => h.GetAsync<ApplicationDTO>(
                It.Is<string>(url => url == "http://test.api/application/autodecision-info/" + loanNumber),
                It.IsAny<Dictionary<string, string>>()), Times.Once);
        }

        [Fact]
        public async Task GetLastApplicationsReturnsLastApplicationsWhenHttpCallSucceeds()
        {
            // Arrange
            int customerId = 1;
            var loanNumber = "12345678";
            var mockLastApplicationsDto = new List<LastApplicationDTO>
            {
                new LastApplicationDTO
                {
                    LoanNumber = "11111111",
                    Type = ApplicationType.Refi,
                    AmountOfPayment = 180,
                    Status = ApplicationStatus.Booked,
                    BankRoutingNumber = "55555",
                    BankAccountNumber = "987654321",
                    BrowserFingerprint = "abcde1fghij2klmno3pqrst4uwvx5yz6",
                    CreatedAt = DateTime.Now.AddDays(-30)
                },
                new LastApplicationDTO
                {
                    LoanNumber = "22222222",
                    Type = ApplicationType.NewLoan,
                    AmountOfPayment = 180,
                    Status = ApplicationStatus.Liquidated,
                    BankRoutingNumber = "55555",
                    BankAccountNumber = "987654321",
                    BrowserFingerprint = "abcde1fghij2klmno3pqrst4uwvx5yz6",
                    CreatedAt = DateTime.Now.AddDays(-60)
                }
            };

            _mockConfiguration.Setup(c => c["Apis:CustomerInfoApi"]).Returns("http://test.api");

            var httpResponse = (
                Success: true,
                StatusCode: HttpStatusCode.OK,
                Response: mockLastApplicationsDto);

            _mockHttpService.Setup(h =>
                    h.GetAsync<List<LastApplicationDTO>>(It
                        .Is<string>(url => url == "http://test.api/application/last-applications?customerId=" + customerId + "&loanNumber=" + loanNumber),
            It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(httpResponse);

            var repository = new ApplicationRepository(_mockConfiguration.Object, _mockHttpService.Object, _mockLogger.Object);

            // Act
            var result = await repository.GetLastApplications(customerId, loanNumber);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<LastApplication>>(result);
            Assert.Equal(2, result.Count);

            for (int i = 0; i < result.Count; i++)
            {
                //Assert.Equal(customerId, result[i].CustomerId));
                Assert.Equal(mockLastApplicationsDto[i].LoanNumber, result[i].LoanNumber);
                Assert.Equal(mockLastApplicationsDto[i].Type, result[i].Type);
                Assert.Equal(mockLastApplicationsDto[i].AmountOfPayment, result[i].AmountOfPayment);
                Assert.Equal(mockLastApplicationsDto[i].Status, result[i].Status);
                Assert.Equal(mockLastApplicationsDto[i].BankRoutingNumber, result[i].BankRoutingNumber);
                Assert.Equal(mockLastApplicationsDto[i].BankAccountNumber, result[i].BankAccountNumber);
                Assert.Equal(mockLastApplicationsDto[i].BrowserFingerprint, result[i].BrowserFingerprint);
                Assert.Equal(mockLastApplicationsDto[i].CreatedAt, result[i].CreatedAt);
            }

            _mockHttpService.Verify(h => h.GetAsync<List<LastApplicationDTO>>(
                It.Is<string>(url => url == "http://test.api/application/last-applications?customerId=" + customerId + "&loanNumber=" + loanNumber),
                It.IsAny<Dictionary<string, string>>()), Times.Once);
        }

    }
}
