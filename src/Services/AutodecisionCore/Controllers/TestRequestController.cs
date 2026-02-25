using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Data.Repositories.Interfaces;
using AutodecisionCore.DTOs;
using AutodecisionCore.Events;
using AutodecisionCore.Services.Interfaces;
using BmgMoney.MessageClient.Core.Interfaces;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;

namespace AutodecisionCore.Controllers
{
    [Route("testRequest")]
    [ApiController]
    public class TestRequestController : BaseController
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ICreditPolicyRepository _creditPolicyRepository;
        private readonly IOpenBankingRepository _openBankingRepository;
        private readonly IFactorTrustRepository _factorTrustRepository;
        private readonly ITopicClient _topicClient;
        private readonly IApplicationCoreRepository _applicationCoreRepository;
        private readonly IFinalValidationService _finalValidationService;
        private readonly INewCreditPolicyRepository _newCreditPolicyRepository;
        private readonly IFacetecRepository _facetecRepository;
        private readonly INewOpenBankingRepository _newOpenBankingRepository;
        private readonly IFlagHelperRepository _flagHelperRepository;
        private readonly IEmployerRepository _employerRepository;

        public TestRequestController(
            IApplicationRepository applicationRepository,
            ICustomerRepository customerRepository,
            ICreditPolicyRepository creditPolicyRepository,
            IOpenBankingRepository openBankingRepository,
            IFactorTrustRepository factorTrustRepository,
            ITopicClient topicClient,
            IApplicationCoreRepository applicationCoreRepository,
            IFacetecRepository facetecRepository,
            IFinalValidationService finalValidationService,
            INewCreditPolicyRepository newCreditPolicyRepository,
            INewOpenBankingRepository newOpenBankingRepository,
            IFlagHelperRepository flagHelperRepository,
            IEmployerRepository employerRepository
            )
        {
            _applicationRepository = applicationRepository;
            _customerRepository = customerRepository;
            _creditPolicyRepository = creditPolicyRepository;
            _openBankingRepository = openBankingRepository;
            _factorTrustRepository = factorTrustRepository;
            _topicClient = topicClient;
            _applicationCoreRepository = applicationCoreRepository;
            _finalValidationService = finalValidationService;
            _newCreditPolicyRepository = newCreditPolicyRepository;
            _flagHelperRepository = flagHelperRepository;
            _facetecRepository = facetecRepository;
            _newOpenBankingRepository = newOpenBankingRepository;
            _employerRepository = employerRepository;
        }

        [HttpGet]
        [Route("applicationInfo/{loanNumber}")]
        public async Task<IActionResult> GetApplicationInfo([FromRoute] string loanNumber)
        {
            var response = await _applicationRepository.GetApplicationInfo(loanNumber);
            return SuccessWithData(response);
        }

        [HttpGet]
        [Route("customerInfo/{customerId}")]
        public async Task<IActionResult> GetGustomerInfo([FromRoute] int customerId)
        {
            if (customerId == 0)
                return BadRequestWithMessage("Customer cannot be null.");

            var response = await _customerRepository.GetCustomerInfo(customerId);
            return SuccessWithData(response);
        }

        [HttpGet]
        [Route("creditPolicyInfo")]
        public async Task<IActionResult> GetCreditPolicyInfo([FromQuery] string employerKey, [FromQuery] string productKey, [FromQuery] string stateAbbreviation, [FromQuery] string applicationType)
        {
            var response = await _creditPolicyRepository.GetCreditPolicyInfo(employerKey, productKey, stateAbbreviation, applicationType);
            return SuccessWithData(response);
        }

        [HttpGet]
        [Route("newCreditPolicyInfo")]
        public async Task<IActionResult> GetNewCreditPolicyInfo([FromQuery] string loanNumber)
        {
            var application = await _applicationRepository.GetApplicationInfo(loanNumber);
            if (application == null)
                return BadRequestWithMessage("Application not found");

            var response = await _newCreditPolicyRepository.GetNewCreditPolicyAndRules(application);
            if (response == null)
                return BadRequestWithMessage("Credit Policy not found");

            return SuccessWithData(response);
        }

        [HttpGet]
        [Route("newOpenBankingInfo/{customerId}")]
        public async Task<IActionResult> GetNewOpenBankingInfo([FromRoute] int customerId)
        {
            var response = await _newOpenBankingRepository.GetNewOpenBanking(customerId);
            if (response == null)
                return BadRequestWithMessage("Open Banking not found");

            return SuccessWithData(response);
        }

        [HttpGet]
        [Route("openBankingInfo/{customerId}")]
        public async Task<IActionResult> GetOpenBankingInfo([FromRoute] int customerId)
        {
            var response = await _openBankingRepository.GetOpenBankingInfo(customerId);

            return SuccessWithData(response);
        }

        [HttpGet]
        [Route("factorTrustInfo")]
        public async Task<IActionResult> GetFactorTrustInfo([FromQuery] int customerId, [FromQuery] string loanNumber)
        {
            var response = await _factorTrustRepository.GetFactorTrustInfo(customerId, loanNumber);

            return SuccessWithData(response);
        }

        [HttpPost]
        [Route("approveApplicationHandler")]
        public async Task<IActionResult> ApproveApplication([FromBody] ApproveApplicationRequestEvent request)
        {
            var r = new ApproveApplicationRequestEvent()
            {
                LoanNumber = request.LoanNumber
            };

            await _topicClient.Produce<Null, ApproveApplicationRequestEvent>(Constants.Topics.NotifyAprove, new Message<Null, ApproveApplicationRequestEvent>()
            {
                Value = r
            });

            return Ok();
        }

        [HttpPost]
        [Route("declineApplicationHandler")]
        public async Task<IActionResult> DeclineApplication([FromBody] DeclineApplicationRequestEvent request)
        {
            var r = new DeclineApplicationRequestEvent()
            {
                LoanNumber = request.LoanNumber
            };

            await _topicClient.Produce<Null, DeclineApplicationRequestEvent>(Constants.Topics.NotifyDecline, new Message<Null, DeclineApplicationRequestEvent>()
            {
                Value = r
            });

            return Ok();
        }

        [HttpPost]
        [Route("requireDocumentsHandler")]
        public async Task<IActionResult> RequireDocuments([FromBody] RequireDocumentsRequestEvent request)
        {
            var r = new RequireDocumentsRequestEvent()
            {
                LoanNumber = request.LoanNumber,
                Flags = request.Flags
            };

            await _topicClient.Produce<Null, RequireDocumentsRequestEvent>(Constants.Topics.NotifyPending, new Message<Null, RequireDocumentsRequestEvent>()
            {
                Value = r
            });

            return Ok();
        }

        [HttpPost]
        [Route("requireAllotment")]
        public async Task<IActionResult> RequireAllotment([FromBody] RequireAllotmentRequestEvent request)
        {
            var r = new RequireAllotmentRequestEvent()
            {
                LoanNumber = request.LoanNumber,
                PaymentType = request.PaymentType
            };

            await _topicClient.Produce<Null, RequireAllotmentRequestEvent>(Constants.Topics.NotifyAllotment, new Message<Null, RequireAllotmentRequestEvent>()
            {
                Value = r
            });

            return Ok();
        }

        [HttpPost]
        [Route("processFlagRequest")]
        public async Task<IActionResult> ProcessFlagRequest([FromBody] FlagRequestDTO request)
        {
            var r = new ProcessFlagRequestEvent()
            {
                LoanNumber = request.LoanNumber,
                Key = $"{Constants.Topics.FlagRequestPrefix}{request.LoanNumber}"
            };

            await _topicClient.Produce<Null, ProcessFlagRequestEvent>(
                $"{Constants.Topics.FlagRequestPrefix}{request.FlagCode}", new Message<Null, ProcessFlagRequestEvent>() { Value = r });

            return Ok();
        }


        [HttpPost]
        [Route("processFlagResponse")]
        public async Task<IActionResult> ProcessFlagResponse([FromBody] ProcessFlagResponseEvent request, [FromServices] IApplicationFlagsService flagservice)
        {
            var applicationCore = await _applicationCoreRepository.FindByLoanNumberIncludeApplicationFlagsAsync(request.LoanNumber);
            await flagservice.FlagResponseStatusRegister(request, applicationCore);

            return Ok();
        }

        [HttpPost]
        [Route("defaultDocuments")]
        public async Task<IActionResult> DefaultDocuments([FromBody] NotifyDefaultDocumentsRequestEvent request)
        {
            var r = new NotifyDefaultDocumentsRequestEvent()
            {
                LoanNumber = request.LoanNumber
            };

            await _topicClient.Produce<Null, NotifyDefaultDocumentsRequestEvent>(Constants.Topics.NotifyPendingDefaultDocuments, new Message<Null, NotifyDefaultDocumentsRequestEvent>()
            {
                Value = r
            });

            return Ok();
        }


        [HttpPost]
        [Route("SendRequestMessages")]
        public async Task<IActionResult> SendRequestMessages([FromServices] IAutodecisionPublisherService _service)
        {
            var applicationCore = new ApplicationCore("123456789");

            applicationCore.AddFlag("217", false);
            applicationCore.AddFlag("218", false);
            applicationCore.AddFlag("220", false);
            applicationCore.AddFlag("225", false);

            await _service.PublishTheFlagsRequest(applicationCore, "abcde", string.Empty);

            return Ok();
        }

        [HttpPost]
        [Route("processFinalValidation/{loanNumber}")]
        public async Task<IActionResult> ProcessFlagResponse([FromRoute] string loanNumber)
        {
            var applicationCore = await _applicationCoreRepository.FindByLoanNumberAsync(loanNumber);

            var processFinalValidation = new ProcessFinalValidation()
            {
                LoanNumber = loanNumber,
                Key = $"{Constants.Topics.RedisKeyPrefix}{loanNumber}",
                Version = 1
            };

            await _finalValidationService.Process(processFinalValidation, applicationCore);

            return Ok();
        }


        [HttpPost]
        [Route("processApplication/{loanNumber}")]
        public async Task<IActionResult> ProcessApplication([FromRoute] string loanNumber)
        {
            if (string.IsNullOrEmpty(loanNumber))
                return BadRequestWithMessage("Loan Number is required");

            var message = new Message<Null, ProcessApplicationRequestEvent>()
            {
                Value = new ProcessApplicationRequestEvent()
                {
                    LoanNumber = loanNumber,
                    Reason = "old-autodecision",
                    RequestedAt = DateTime.Now
                }
            };

            await _topicClient.Produce("autodecision-process", message);
            return Ok();
        }

        [HttpGet]
        [Route("faceid/{customerId}")]
        public async Task<IActionResult> GetFaceId([FromRoute] int customerId)
        {
            if (customerId == 0)
                return BadRequestWithMessage("Customer ID is required");

            var data = await _facetecRepository.GetFaceRecognition(customerId);
            return SuccessWithData(data);
        }

        [HttpGet("flag-helper")]
        public async Task<IActionResult> GetFlagHelperInformationAsync([FromQuery] int customerId, [FromQuery] int employerId, [FromQuery] int applicationId)
        {
            if (customerId == 0 || employerId == 0 || applicationId == 0)
                return BadRequestWithMessage("Parameters are required!");

            var data = await _flagHelperRepository.GetFlagHelperInformationAsync(customerId, employerId, applicationId);
            return Ok(data);
        }

        [HttpGet("employer")]
        public async Task<IActionResult> GetEmployerInformationAsync([FromQuery] int employerId)
        {
            if (employerId == 0)
                return BadRequestWithMessage("Parameters are required!");

            var data = await _employerRepository.GetEmployerAsync(employerId);
            return Ok(data);
        }

        [HttpGet("application-income")]
        public async Task<IActionResult> GetApplicationIncomeInformationAsync([FromQuery] int applicationId)
        {
            if (applicationId == 0)
                return BadRequestWithMessage("Parameters are required!");

            var data = await _applicationRepository.GetTotalIncomeDetailsByApplicationIdAsync(applicationId);
            return Ok(data);
        }
    }
}