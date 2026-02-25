using AutodecisionCore.Contracts.Messages;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Handlers;
using BmgMoney.LogClient.Extensions;
using BMGMoney.SDK.V2.Environments;
using Confluent.Kafka;
using Elastic.Apm.NetCoreAll;
using MassTransit;
using Newtonsoft.Json;
using static AutodecisionCore.Contracts.Constants.Constants;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services
    .AddControllers()
    .AddNewtonsoftJson(o =>
    {
        o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.RegisterApplicationServices(builder.Configuration);

builder.WebHost.UseUrls("http://0.0.0.0:5001");

builder.WebHost.UseSentry();

const string MultipleflagsClientId = "multipleflags";

builder.Services.AddMassTransit(bus =>
{
    bus.UsingInMemory((context, cfg) => { });

    var kafkaServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS");
    var KafkaUser = Environment.GetEnvironmentVariable("KAFKA_SASL_USERNAME");
    var KafkaPsw = Environment.GetEnvironmentVariable("KAFKA_SASL_PASSWORD");

    bus.AddRider(rider =>
    {
        rider.UsingKafka((context, k) =>

        {
            k.Host(kafkaServers, h =>
            {
                h.UseSasl(sasl =>
                {
                    sasl.Username = KafkaUser;
                    sasl.Password = KafkaPsw;
                    sasl.Mechanism = SaslMechanism.Plain;
                    sasl.SecurityProtocol = SecurityProtocol.SaslSsl;
                });
            });

            var clientId = MultipleflagsClientId;

            k.ClientId = clientId;

            BasicsRegisterKafkaConsumers(context, k);
            OpenConnectionsRegisterKafkaConsumers(context, k);
            ExternalRegisterKafkaConsumers(context, k);
            LawRegisterKafkaConsumers(context, k);
            LongRuningRegisterKafkaConsumers(context, k);
            CustomerIdentityRegisterKafkaConsumers(context, k);
        });

        rider.AddConsumer<PhoneVoipHandler>();
        rider.AddConsumer<BrowserFringerprintHandler>();
        rider.AddConsumer<OneLoanPerSSNHandler>();
        rider.AddConsumer<BankInfoFoundHandler>();
        rider.AddConsumer<RoutingNumberHandler>();
        rider.AddConsumer<DifferentStatesGeolocationHandler>();
        rider.AddConsumer<InternetBankHandler>();
        rider.AddConsumer<FoundOnBlockListHandler>();
        rider.AddConsumer<FoundOnWhiteListHandler>();
        rider.AddConsumer<DifferentStatesHandler>();
        rider.AddConsumer<OpenPayrollSSNDoesNotMatchHandler>();
        rider.AddConsumer<HouseholdHitHandler>();
        rider.AddConsumer<OpenBankingPayrollNotConnectedHandler>();
        rider.AddConsumer<OpenPayrollNotConnectedHandler>();
        rider.AddConsumer<OpenBankingNotConnectedHandler>();
        rider.AddConsumer<PhoneValidationHandler>();
        rider.AddConsumer<DailyReceivingsHandler>();
        rider.AddConsumer<SimilarCustomerHandler>();
        rider.AddConsumer<FirstnetCreditHistoryHandler>();
        rider.AddConsumer<TUBankruptcyHandler>();
        rider.AddConsumer<CustomerAuthenticationHandler>();
        rider.AddConsumer<GrossPayHandler>();
        rider.AddConsumer<ReverseCensusHandler>();
        rider.AddConsumer<ActiveMilitaryDutyHandler>();
        rider.AddConsumer<TUCriticalAndInternetBankHandler>();
        rider.AddConsumer<EligibilityRuleHandler>();
        rider.AddConsumer<GiactHandler>();
        rider.AddConsumer<DebitCardBankAccountAnalysisHandler>();
        rider.AddConsumer<NoTUResponseHandler>();
        rider.AddConsumer<EmploymentLengthHandler>();
        rider.AddConsumer<CustomerIdentificationHandler>();
        rider.AddConsumer<TransunionScoresHandler>();
        rider.AddConsumer<AgenciesEligibilityRuleHandler>();
        rider.AddConsumer<AllotmentRuleHandler>();
        rider.AddConsumer<CreditPolicyIsMissingHandler>();
        rider.AddConsumer<FraudAlertHandler>();
        rider.AddConsumer<MandatoryHRVerificationHandler>();
        rider.AddConsumer<PartnerAssociationHandler>();
        rider.AddConsumer<FactorTrustInconsistencyHandler>();
        rider.AddConsumer<ProbableCashAppHandler>();
        rider.AddConsumer<OpenPayrollInconsistencyHandler>();
        rider.AddConsumer<LevelOfCommitmentHandler>();
        rider.AddConsumer<IncomeValidationHandler>();
        rider.AddConsumer<OFACClarityHandler>();
        rider.AddConsumer<PhoneValidatedHandler>();
        rider.AddProducer<ProcessFlagResponseEvent>(Topics.FlagResponse);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!BMGMoneyEnvironment.IsLocal())
    app.UseAllElasticApm(builder.Configuration);

app.UseBmgMoneyLogger();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

static void LawRegisterKafkaConsumers(
    IRiderRegistrationContext context,
    IKafkaFactoryConfigurator k)
{
    var clientId = MultipleflagsClientId + "-law";

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.MandatoryHRVerification}", clientId, e =>
    {
        e.ConfigureConsumer<MandatoryHRVerificationHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.AgenciesEligibilityRule}", clientId, e =>
    {
        e.ConfigureConsumer<AgenciesEligibilityRuleHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.ReverseCensus}", clientId, e =>
    {
        e.ConfigureConsumer<ReverseCensusHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.EligibilityRule}", clientId, e =>
    {
        e.ConfigureConsumer<EligibilityRuleHandler>(context);
    });
}

static void OpenConnectionsRegisterKafkaConsumers(
    IRiderRegistrationContext context,
    IKafkaFactoryConfigurator k)
{
    var clientId = MultipleflagsClientId + "-openconnections";

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.OpenPayrollSSNDoesNotMatch}", clientId, e =>
    {
        e.ConfigureConsumer<OpenPayrollSSNDoesNotMatchHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.OpenBankingOrPayrollNotConnected}", clientId, e =>
    {
        e.ConfigureConsumer<OpenBankingPayrollNotConnectedHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.OpenPayrollNotConnected}", clientId, e =>
    {
        e.ConfigureConsumer<OpenPayrollNotConnectedHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.OpenBankingNotConnected}", clientId, e =>
    {
        e.ConfigureConsumer<OpenBankingNotConnectedHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.DebitCardBankAccountAnalysis}", clientId, e =>
    {
        e.ConfigureConsumer<DebitCardBankAccountAnalysisHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.DailyReceivings}", clientId, e =>
    {
        e.ConfigureConsumer<DailyReceivingsHandler>(context);
    });
}

static void ExternalRegisterKafkaConsumers(
    IRiderRegistrationContext context,
    IKafkaFactoryConfigurator k)
{
    var clientId = MultipleflagsClientId + "-external";

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.ClarityOFACHIT}", clientId, e =>
    {
        e.ConfigureConsumer<OFACClarityHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.TUBankruptcy}", clientId, e =>
    {
        e.ConfigureConsumer<TUBankruptcyHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.TransunionScores}", clientId, e =>
    {
        e.ConfigureConsumer<TransunionScoresHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.TUCriticalAndInternetBank}", clientId, e =>
    {
        e.ConfigureConsumer<TUCriticalAndInternetBankHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.NoTUResponse}", clientId, e =>
    {
        e.ConfigureConsumer<NoTUResponseHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.Giact}", clientId, e =>
    {
        e.ConfigureConsumer<GiactHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.ActiveMilitaryDuty}", clientId, e =>
    {
        e.ConfigureConsumer<ActiveMilitaryDutyHandler>(context);
    });
}


static void LongRuningRegisterKafkaConsumers(
    IRiderRegistrationContext context,
    IKafkaFactoryConfigurator k)
{
    var clientId = MultipleflagsClientId + "-long-runing";

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.HouseHoldHit}", clientId, e =>
    {
        e.ConfigureConsumer<HouseholdHitHandler>(context);
    });
}

static void CustomerIdentityRegisterKafkaConsumers(
    IRiderRegistrationContext context,
    IKafkaFactoryConfigurator k)
{
    var clientId = MultipleflagsClientId + "-customer-identity";

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.CustomerAuthentication}", clientId, e =>
    {
        e.ConfigureConsumer<CustomerAuthenticationHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.CustomerIdentityFlag}", clientId, e =>
    {
        e.ConfigureConsumer<CustomerIdentificationHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.SimilarCustomer}", clientId, e =>
    {
        e.ConfigureConsumer<SimilarCustomerHandler>(context);
    });
}

static void BasicsRegisterKafkaConsumers(
    IRiderRegistrationContext context,
    IKafkaFactoryConfigurator k)
{
    var clientId = MultipleflagsClientId + "-basics";

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.PhoneVoip}", clientId, e =>
    {
        e.ConfigureConsumer<PhoneVoipHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.BrowserFingerprint}", clientId, e =>
    {
        e.ConfigureConsumer<BrowserFringerprintHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.OneLoanPerSSN}", clientId, e =>
    {
        e.ConfigureConsumer<OneLoanPerSSNHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.BankInfoFound}", clientId, e =>
    {
        e.ConfigureConsumer<BankInfoFoundHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.RoutingNumberVerification}", clientId, e =>
    {
        e.ConfigureConsumer<RoutingNumberHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.DifferentStatesGeolocation}", clientId, e =>
    {
        e.ConfigureConsumer<DifferentStatesGeolocationHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.InternetBank}", clientId, e =>
    {
        e.ConfigureConsumer<InternetBankHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.FoundOnBlockList}", clientId, e =>
    {
        e.ConfigureConsumer<FoundOnBlockListHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.FoundOnWhiteList}", clientId, e =>
    {
        e.ConfigureConsumer<FoundOnWhiteListHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.DifferentStates}", clientId, e =>
    {
        e.ConfigureConsumer<DifferentStatesHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.PhoneValidation}", clientId, e =>
    {
        e.ConfigureConsumer<PhoneValidationHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.PhoneValidated}", clientId, e =>
    {
        e.ConfigureConsumer<PhoneValidatedHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.IncomeValidation}", clientId, e =>
    {
        e.ConfigureConsumer<IncomeValidationHandler>(context);
    });


    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.LevelOfCommitment}", clientId, e =>
    {
        e.ConfigureConsumer<LevelOfCommitmentHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.OpenPayrollInconsistency}", clientId, e =>
    {
        e.ConfigureConsumer<OpenPayrollInconsistencyHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.ProbableCashApp}", clientId, e =>
    {
        e.ConfigureConsumer<ProbableCashAppHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.FactorTrustInconsistency}", clientId, e =>
    {
        e.ConfigureConsumer<FactorTrustInconsistencyHandler>(context);
    });


    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.GrossPay}", clientId, e =>
    {
        e.ConfigureConsumer<GrossPayHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.EmploymentLength}", clientId, e =>
    {
        e.ConfigureConsumer<EmploymentLengthHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.AllotmentValidationOptionTwo}", clientId, e =>
    {
        e.ConfigureConsumer<AllotmentRuleHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.AllotmentValidationOptionOne}", clientId, e =>
    {
        e.ConfigureConsumer<AllotmentRuleHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.FirstnetCreditHistory}", clientId, e =>
    {
        e.ConfigureConsumer<FirstnetCreditHistoryHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.CreditPolicyIsMissing}", clientId, e =>
    {
        e.ConfigureConsumer<CreditPolicyIsMissingHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.FraudAlert}", clientId, e =>
    {
        e.ConfigureConsumer<FraudAlertHandler>(context);
    });

    k.TopicEndpoint<ProcessFlagRequestEvent>($"{Topics.FlagRequestPrefix}{FlagCode.PartnerAssociation}", clientId, e =>
    {
        e.ConfigureConsumer<PartnerAssociationHandler>(context);
    });

}