using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Handlers;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using AutodecisionMultipleFlagsProcessor.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Castle.Core.Resource;
using AutodecisionMultipleFlagsProcessor.DTOs;
using AutodecisionMultipleFlagsProcessor.Tests.TestHelper;

namespace AutodecisionMultipleFlagsProcessor.Tests
{
    public class SimilarCustomerHandlerTests
    {

        private readonly Mock<ILogger<SimilarCustomerHandler>> _mockLogger;
        private readonly Mock<IFlagHelper> _mockFlagHelper;
        private readonly Mock<ICustomerInfo> _mockCustomerInfo;
        public SimilarCustomerHandlerTests()
        {
            _mockLogger = new Mock<ILogger<SimilarCustomerHandler>>();
            _mockFlagHelper = new Mock<IFlagHelper>();
            _mockCustomerInfo = new Mock<ICustomerInfo>();

            FlagHelperMockUtility.AddDefaultBehaviors(_mockFlagHelper);
        }

        [Fact]
        public void WhenSimlarCustomerIsFound()
        {
            var obj = new AutodecisionCompositeData()
            {
                Application = new Application
                {
                     Status = "2"   
                },
                Customer = new Customer()
                {
                    Id = 12,
                    FirstName = "Teste Nome",
                    LastName = "Teste Sobrenome"
                }
            };


            var similarCustomerDtos = new List<SimilarCustomerDto>()
            {
                new SimilarCustomerDto
                {
                    first_name = "Teste Nome",
                    last_name = "Teste Sobrenome",
                    levenshtein_distance = 0,
                    loan_number = "11111",
                    name_similarity = 1,
                    ssn = "1111",
                    ssn_similarity = 222
                }
            };

            var handler = new SimilarCustomerHandler(_mockLogger.Object, _mockFlagHelper.Object, _mockCustomerInfo.Object);

            _mockCustomerInfo.Setup(_ => _.GetSimilarCustomers(obj.Customer.Id))
                .Returns(Task.FromResult(similarCustomerDtos));

            var resposta = handler.ProcessFlag(obj);

            Assert.Equal(FlagResultEnum.PendingApproval, resposta.FlagResult);
        }

    }
}
