using AutodecisionCore.AutoApprovalCore.Interface;
using AutodecisionCore.Core.AutoApprovalCore;
using AutodecisionCore.Core.HandleFlagStrategies;
using AutodecisionCore.Core.HandleFlagStrategies.Interfaces;
using AutodecisionCore.Data.Context;
using AutodecisionCore.Data.HttpRepositories;
using AutodecisionCore.Data.HttpRepositories.Interfaces;
using AutodecisionCore.Data.Infra;
using AutodecisionCore.Data.Repositories;
using AutodecisionCore.Data.Repositories.Interfaces;
using AutodecisionCore.Services;
using AutodecisionCore.Services.HttpServices;
using AutodecisionCore.Services.Interfaces;
using BmgMoney.FeatureToggle.DotNetCoreClient.Client;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using BMGMoney.SDK.V2.Extensions;
using Google.Cloud.Diagnostics.AspNetCore;
using Google.Cloud.Diagnostics.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using System.Net;

namespace AutodecisionCore.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.AddHttpContextAccessor();
            services.AddHttpClient();
            services.AddHttpService();
            RegisterDatabase(services, configuration);
            RegisterRepositories(services, configuration);
            RegisterServices(services, configuration);
            //AddTracingAndLogging(services, configuration);
            RegisterRedis(services, configuration);
            RegisterHandleFlagStrategies(services);
            services.AddMemoryCache();
			services.AddBmgMoneyLogger(configuration);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AutodecisionCore", Version = "v1" });
            });
        }

        private static void RegisterDatabase(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var database = configuration![$"databases:default:database"]!;
            string defaultDBConnection = GetConnectionString(database, configuration);

            services.AddDbContext<DatabaseContext>(
                o => o.UseMySql(defaultDBConnection, ServerVersion.AutoDetect(defaultDBConnection))
            );

            string bmgMoneyDBConnection = GetConnectionString(database, configuration);

            var databaseConnectionHelper = new DatabaseConnectionHelper()
            {
                BmgMoneyConectionString = bmgMoneyDBConnection
            };

            services.AddSingleton(databaseConnectionHelper);
            services.AddScoped<DbContext, DatabaseContext>();
        }

        private static void RegisterRedis(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? configuration["ConnectionStrings:RedisServer"];

            services.AddRedisCache(new ConfigurationOptions()
            {
                KeepAlive = 0,
                AllowAdmin = true,
                EndPoints = { redisHost },
                ConnectTimeout = 5000,
                ConnectRetry = 5,
                SyncTimeout = 5000,
                AbortOnConnectFail = false,
            });
        }

        public static void AddTracingAndLogging(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {

			if (string.IsNullOrEmpty(configuration["GCP:ProjectId"]))
            {
                Console.WriteLine("GCP:ProjectId is empty. Tracing is disabled.");
                return;
            }

            var googleCredentials = Environment.GetEnvironmentVariable(
                "GOOGLE_APPLICATION_CREDENTIALS"
            );
            if (string.IsNullOrEmpty(googleCredentials))
            {
                Console.WriteLine("GOOGLE_APPLICATION_CREDENTIALS is empty. Tracing is disabled.");
                return;
            }

            services.AddGoogleTraceForAspNetCore(
                new AspNetCoreTraceOptions
                {
                    ServiceOptions = new Google.Cloud.Diagnostics.Common.TraceServiceOptions
                    {
                        // Replace ProjectId with your Google Cloud Project ID.
                        ProjectId = configuration["GCP:ProjectId"],
                        Options = TraceOptions.Create(bufferOptions: BufferOptions.NoBuffer())
                    }
                }
            );
        }

        public static string GetConnectionString(string databaseName, IConfiguration? configuration)
        {
            string? host = Environment.GetEnvironmentVariable("DB_HOST");
            string? user = Environment.GetEnvironmentVariable("DB_USER");
            string? password = Environment.GetEnvironmentVariable("DB_PASSWORD");
            string? sslMode = Environment.GetEnvironmentVariable("DB_SSL_MODE") ?? "None";

            if (string.IsNullOrEmpty(user) && configuration != null)
                user = configuration[$"databases:default:user"];
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user))
                throw new Exception("Some database configurations were not found.");
            if (string.IsNullOrEmpty(password))
                return $"Server={host}; Database={databaseName}; Uid={user};SSL Mode={sslMode};";
            return $"Server={host}; Database={databaseName}; Uid={user};Pwd={password}; SSL Mode={sslMode};";
        }

        private static void RegisterRepositories(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.AddScoped<IHealthCheckRepository, HealthCheckRepository>();
            services.AddScoped<IApplicationRepository, ApplicationRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ICensusRepository, CensusRepository>();
            services.AddScoped<IOpenPayrollRepository, OpenPayrollRepository>();
            services.AddScoped<ICreditPolicyRepository, CreditPolicyRepository>();
            services.AddScoped<IOpenBankingRepository, OpenBankingRepository>();
            services.AddScoped<IFactorTrustRepository, FactorTrustRepository>();
            services.AddScoped<IApplicationCoreRepository, ApplicationCoreRepository>();
            services.AddScoped<IFlagRepository, FlagRepository>();
            services.AddScoped<IAutoApprovalFundingMethodRepository, AutoApprovalFundingMethodRepository>();
            services.AddScoped<IAutoApprovalPaymentTypeRepository, AutoApprovalPaymentTypeRepository>();
            services.AddScoped<IAutoApprovalUwClusterRepository, AutoApprovalUwClusterRepository>();
            services.AddScoped<IDebitCardRepository, DebitCardRepository>();
            services.AddScoped<ITransunionRepository, TransunionRepository>();
            services.AddScoped<IBlockListRepository, BlockListRepository>();
            services.AddScoped<IWhiteListRepository, WhiteListRepository>();
            services.AddScoped<IClarityRepository, ClarityRepository>();
            services.AddScoped<IApplicationWarningRepository, ApplicationWarningRepository>();
            services.AddScoped<IFacetecRepository, FacetecRepository>();
            services.AddScoped<ITriggerRepository, TriggerRepository>();
            services.AddScoped<IDashboardRepository, DashboardRepository>();
            services.AddScoped<INewCreditPolicyRepository, NewCreditPolicyRepository>();
            services.AddScoped<IDeclineReasonFlagsRepository, DeclineReasonFlagsRepository>();
            services.AddScoped<INewOpenBankingRepository, NewOpenBankingRepository>();
            services.AddScoped<IFlagHelperRepository, FlagHelperRepository>();
            services.AddScoped<ICreditRiskRepository, CreditRiskRepository>();
            services.AddScoped<IEmployerRepository, EmployerRepository>();
        }

        private static void RegisterServices(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            AddRedlock(services);
            services.AddScoped<IHealthCheckService, HealthCheckService>();
            services.AddScoped<IAutodecisionCompositeService, AutodecisionCompositeService>();
            services.AddScoped<IFinalValidationService, FinalValidationService>();
            services.TryAddSingleton<IFeatureToggleClient>(new FeatureToggleClient(configuration["feature_toggle:url"], configuration["feature_toggle:environment"]));
            services.AddScoped<IApplicationCoreService, ApplicationCoreService>();
            services.AddScoped<IApplicationFlagsService, ApplicationFlagsService>();
            services.AddScoped<IAutoApprovalService, AutoApprovalService>();
            services.AddScoped<IAutoApprovalManager, AutoApprovalManager>();
            services.AddScoped<IExternalValidationService, ExternalValidationService>();
            services.AddScoped<ICreditInquiryService, CreditInquiryService>();
            services.AddScoped<IAutodecisionPublisherService, AutodecisionPublisherService>();
            services.AddScoped<IMemoryCache, MemoryCache>();
            services.AddScoped<IHandlerHelper, HandlerHelper>();
            services.AddScoped<ITriggerService, TriggerService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IFlagsService, FlagsService>();
            services.AddScoped<IDeclineReasonFlagsService, DeclineReasonFlagsService>();
            services.AddScoped<ICreditRiskService, CreditRiskService>();
        }

        private static void RegisterHandleFlagStrategies(this IServiceCollection services)
        {
            services.AddScoped<IApplicationFlagsServiceFactory, ApplicationFlagsServiceFactory>();
            services.AddScoped<IApplicationFlagsBinder, ApplicationFlagsBinder>();
            services.AddScoped<IDefaultFlagsStrategy, DefaultFlagsStrategy>();

            services.AddScoped<IApplicationFlagsStrategy, CashlessFlagsStrategy>();
            services.AddScoped<IApplicationFlagsStrategy, CashbackFlagsStrategy>();
            services.AddScoped<IApplicationFlagsStrategy, RefiFlagsStrategy>();
        }

        private static void AddRedlock(IServiceCollection services)
        {
            var redisAddress = Environment.GetEnvironmentVariable("REDIS_HOST");

            if (string.IsNullOrEmpty(redisAddress))
                throw new Exception("Pubsubmanager: Could not find REDIS_HOST environment variable.");

            var endpoint = new DnsEndPoint(GetRedisHost(redisAddress), GetRedisPort(redisAddress));

            var endpoints = new List<RedLockEndPoint>() { endpoint };

            services.AddSingleton<IDistributedLockFactory, RedLockFactory>(
                x => RedLockFactory.Create(endpoints)
            );
        }

        private static string GetRedisHost(string redisAddress)
            => redisAddress?.Split(":")[0];

        private static int GetRedisPort(string redisAddress)
        {
            var port = redisAddress?.Split(":")[1];

            if (string.IsNullOrEmpty(port))
                return 6379;

            return Convert.ToInt32(port);
        }
    }
}