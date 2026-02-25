using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutodecisionCore.Services.Interfaces;
using BMGMoney.SDK.V2.Cache.Redis;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AutodecisionCore.Controllers
{
    [Route("redis")]
    [ApiController]
    public class RedisController : BaseController
    {
        private readonly IAutodecisionCompositeService _autodecisionCompositeService;
        private readonly ILogger<RedisController> _logger;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IApplicationCoreService _applicationCoreService;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IHandlerHelper _handlerHelper;

        public RedisController(
            IAutodecisionCompositeService autodecisionCompositeService,
            ILogger<RedisController> logger,
            IApplicationRepository applicationRepository,
            IApplicationCoreService applicationCoreService,
            IRedisCacheService redisCacheService,
            IHandlerHelper handlerHelper)
        {
            _autodecisionCompositeService = autodecisionCompositeService;
            _logger = logger;
            _applicationRepository = applicationRepository;
            _applicationCoreService = applicationCoreService;
            _redisCacheService = redisCacheService;
            _handlerHelper = handlerHelper;
        }

        [HttpGet]
        [Route("composite-data/{loanNumber}")]
        public async Task<IActionResult> GetAutodecisionCompositeData([FromRoute] string loanNumber)
        {
            if (string.IsNullOrEmpty(loanNumber))
                return BadRequestWithMessage("Loan Number cannot be null!");

            var application = await _applicationRepository.GetApplicationInfo(loanNumber);
            if (application == null || !_handlerHelper.IsRedisAvailable(application.Status)) 
                return Ok(new AutodecisionCompositeData());

            _logger.LogInformation($"Redis controller getting composite data for LoanNumber: {loanNumber}");
            var redisKey = $"{Constants.Topics.RedisKeyPrefix}{loanNumber}";

            return Ok(await _autodecisionCompositeService.GetCompositeDataFromRedis(redisKey));
        }

        [HttpPost]
        [Route("composite-data/{loanNumber}")]
        public async Task<IActionResult> SetAutodecisionCompositeData([FromRoute] string loanNumber)
        {
            if (string.IsNullOrEmpty(loanNumber))
                return BadRequestWithMessage("Loan Number cannot be null!");

            var applicationCore = await _applicationCoreService.ApplicationCoreRegister(loanNumber);
            var application = await _applicationRepository.GetApplicationInfo(loanNumber);
            var redisContent = await _autodecisionCompositeService.FillCompositeData(application, applicationCore);
            var redisKey = $"{Constants.Topics.RedisKeyPrefix}{loanNumber}";

            applicationCore.SetCustomerInfo($"{redisContent.Customer.FirstName} {redisContent.Customer.LastName}", redisContent.Application.EmployerName, redisContent.Application.StateAbbreviation);

            return Ok(await _redisCacheService.StringSetAsync(redisKey, JsonConvert.SerializeObject(redisContent)));
        }
    }
}
