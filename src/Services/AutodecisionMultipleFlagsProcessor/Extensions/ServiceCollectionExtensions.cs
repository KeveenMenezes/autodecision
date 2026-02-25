using AutodecisionMultipleFlagsProcessor.Data.Context;
using AutodecisionMultipleFlagsProcessor.Data.Repositories;
using AutodecisionMultipleFlagsProcessor.Data.Repositories.Interfaces;
using AutodecisionMultipleFlagsProcessor.Services;
using AutodecisionMultipleFlagsProcessor.Services.Interfaces;
using AutodecisionMultipleFlagsProcessor.Utility;
using BmgMoney.FeatureToggle.DotNetCoreClient.Client;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using BmgMoney.LogClient.Extensions;
using BMGMoney.SDK.V2.Extensions;
using Google.Cloud.Diagnostics.AspNetCore;
using Google.Cloud.Diagnostics.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;

namespace AutodecisionMultipleFlagsProcessor.Extensions
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

            //RegisterFeatureToggleSingleton(services, configuration);
            RegisterDatabase(services, configuration);
            RegisterRepositories(services, configuration);
            RegisterServices(services, configuration);
            //AddTracingAndLogging(services, configuration);
            RegisterRedis(services, configuration);
            services.AddBmgMoneyLogger(configuration);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AutodecisionMultipleFlagsProcessor", Version = "v1" });
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

            var databasebmg_money = configuration![$"databases:default:bmg_money"]!;
            string bmgMoneyDBConnection = GetConnectionString(database, configuration);

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
                return $"Server={host}; Database={databaseName}; Uid={user};SSL Mode={sslMode}; Allow User Variables=true";
            return $"Server={host}; Database={databaseName}; Uid={user};Pwd={password}; SSL Mode={sslMode}; Allow User Variables=true";
        }

        private static void RegisterRepositories(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.AddScoped<IHealthCheckRepository, HealthCheckRepository>();
            services.AddScoped<IHouseholdHitRepository, HouseholdHitRepository>();
        }

        private static void RegisterServices(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.AddScoped<IFlagHelper, FlagHelper>();
            services.AddScoped<ICustomerInfo, CustomerInfo>();
            services.AddScoped<IDebitCard, DebitCard>();
            services.AddScoped<ICreditInquiry, CreditInquiry>();
            services.AddScoped<IFaceId, FaceId>();
            services.AddScoped<IOpenBankingService, OpenBankingService>();
            services.TryAddSingleton<IFeatureToggleClient>(new FeatureToggleClient(configuration["feature_toggle:url"], configuration["feature_toggle:environment"]));
            services.AddScoped<IHealthCheckService, HealthCheckService>();
        }

        //private static void RegisterFeatureToggleSingleton(this IServiceCollection services,
        //    IConfiguration configuration)
        //{
        //    _ = new FeatureToggleSingleton(configuration["feature_toggle:url"], configuration["feature_toggle:environment"]);
        //    services.AddSingleton(FeatureToggleSingleton.GetInstance());
        //    services.AddSingleton<IFeatureToggleClient>(FeatureToggleSingleton.GetInstance());
        //}
    }
}