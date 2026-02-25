using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Events;
using AutodecisionCore.Extensions;
using AutodecisionCore.Handlers;
using Confluent.Kafka;
using MassTransit;
using Newtonsoft.Json;
using Constants = AutodecisionCore.Contracts.Constants.Constants;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services
    .AddControllers(o =>
    {
        o.ModelValidatorProviders.Clear();
    })
    .AddNewtonsoftJson(o =>
    {
        o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    });
;

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory((context, cfg) =>
    {
        cfg.UseMessageRetry(x =>
        {
            x.Interval(3, TimeSpan.FromSeconds(5));
        });
    });

    x.AddLogging();

    var kafkaServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS");
    var KafkaUser = Environment.GetEnvironmentVariable("KAFKA_SASL_USERNAME");
    var KafkaPsw = Environment.GetEnvironmentVariable("KAFKA_SASL_PASSWORD");
    var ClientId = "autodecision-core";
    var scope = "";

    if (args.Length == 1)
    {
        ClientId += "-" + args[0]; // concatenando com o "nome" do consumidor
        scope = args[0];
    }

    Console.WriteLine($"Starting {ClientId} with scope '{scope}'");

    x.AddRider(rider =>
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
                                    });
                                    h.UseSasl(x => x.SecurityProtocol = SecurityProtocol.SaslSsl);
                                });

            k.ClientId = ClientId;

            if (string.IsNullOrEmpty(scope) || scope == "core")
            {
                k.TopicEndpoint<ProcessApplicationRequestEvent>(Constants.Topics.Process, ClientId, e =>
                {
                    e.PartitionAssignmentStrategy = PartitionAssignmentStrategy.Range;
                    e.ConfigureConsumer<AutodecisionProcessHandler>(context);
                });
            }

            if (string.IsNullOrEmpty(scope) || scope == "flagresponse")
            {
                k.TopicEndpoint<ProcessFlagResponseEvent>(Constants.Topics.FlagResponse, ClientId, e =>
                {
                    e.PartitionAssignmentStrategy = PartitionAssignmentStrategy.Range;
                    e.ConfigureConsumer<FlagResponseHandler>(context);
                });
            }

            if (string.IsNullOrEmpty(scope) || scope == "finalevaluation")
            {

                Console.WriteLine($"Starting {ClientId} with scope '{scope}'");

                k.TopicEndpoint<ProcessFinalValidation>(Constants.Topics.FinalEvaluation, ClientId, e =>
                {
                    e.PartitionAssignmentStrategy = PartitionAssignmentStrategy.Range;
                    e.ConfigureConsumer<FinalValidationHandler>(context);
                });
            }

            if (string.IsNullOrEmpty(scope) || scope == "notifications")
            {
                k.TopicEndpoint<ApproveApplicationRequestEvent>(Constants.Topics.NotifyAprove, ClientId, e =>
                {
                    e.PartitionAssignmentStrategy = PartitionAssignmentStrategy.Range;
                    e.ConfigureConsumer<ApproveApplicationHandler>(context);
                });
            }

            if (string.IsNullOrEmpty(scope) || scope == "notifications")
            {
                k.TopicEndpoint<DeclineApplicationRequestEvent>(Constants.Topics.NotifyDecline, ClientId, e =>
                {
                    e.PartitionAssignmentStrategy = PartitionAssignmentStrategy.Range;
                    e.ConfigureConsumer<DeclineApplicationHandler>(context);
                });
            }

            if (string.IsNullOrEmpty(scope) || scope == "notifications")
            {
                k.TopicEndpoint<RequireDocumentsRequestEvent>(Constants.Topics.NotifyPending, ClientId, e =>
                {
                    e.PartitionAssignmentStrategy = PartitionAssignmentStrategy.Range;
                    e.ConfigureConsumer<RequireDocumentsHandler>(context);
                });
            }

            if (string.IsNullOrEmpty(scope) || scope == "notifications")
            {
                k.TopicEndpoint<RequireAllotmentRequestEvent>(Constants.Topics.NotifyAllotment, ClientId, e =>
                {
                    e.PartitionAssignmentStrategy = PartitionAssignmentStrategy.Range;
                    e.ConfigureConsumer<RequireAllotmentHandler>(context);
                });
            }

            if (string.IsNullOrEmpty(scope) || scope == "notifications")
            {
                k.TopicEndpoint<NotifyDefaultDocumentsRequestEvent>(Constants.Topics.NotifyPendingDefaultDocuments, ClientId, e =>
                {
                    e.PartitionAssignmentStrategy = PartitionAssignmentStrategy.Range;
                    e.ConfigureConsumer<NotifyDefaultDocumentsHandler>(context);
                });
            }

            if (string.IsNullOrEmpty(scope) || scope == "notifications")
            {
                k.TopicEndpoint<ConnectedDebitCardEvent>(Topics.DebitCardConnected, ClientId, e =>
                {
                    e.PartitionAssignmentStrategy = PartitionAssignmentStrategy.Range;
                    e.ConfigureConsumer<ConnectedDebitCardHandler>(context);
                });
            }

            if (string.IsNullOrEmpty(scope) || scope == "notifications")
            {
                k.TopicEndpoint<CreatedFaceIdRecordEvent>(Topics.FaceIdCreated, ClientId, e =>
                {
                    e.PartitionAssignmentStrategy = PartitionAssignmentStrategy.Range;
                    e.ConfigureConsumer<CreatedFaceIdHandler>(context);
                });
            }

            if (string.IsNullOrEmpty(scope) || scope == "notifications")
            {
                k.TopicEndpoint<NotifyAllotmentSddReceivedEvent>(Constants.Topics.NotifyAllotmentSddReceived, ClientId, e =>
                {
                    e.PartitionAssignmentStrategy = PartitionAssignmentStrategy.Range;
                    e.ConfigureConsumer<NotifyAllotmentSddReceivedHandler>(context);
                });

            }

        });

        rider.AddConsumer<AutodecisionProcessHandler>();
        rider.AddConsumer<FlagResponseHandler>();
        rider.AddConsumer<FinalValidationHandler>();
        rider.AddConsumer<ApproveApplicationHandler>();
        rider.AddConsumer<DeclineApplicationHandler>();
        rider.AddConsumer<RequireDocumentsHandler>();
        rider.AddConsumer<RequireAllotmentHandler>();
        rider.AddConsumer<NotifyDefaultDocumentsHandler>();
        rider.AddConsumer<ConnectedDebitCardHandler>();
        rider.AddConsumer<CreatedFaceIdHandler>();
        rider.AddConsumer<NotifyAllotmentSddReceivedHandler>();

        rider.AddProducer<ProcessFinalValidation>(Constants.Topics.FinalEvaluation);
        rider.AddProducer<ApproveApplicationRequestEvent>(Constants.Topics.NotifyAprove);
        rider.AddProducer<DeclineApplicationRequestEvent>(Constants.Topics.NotifyDecline);
        rider.AddProducer<RequireDocumentsRequestEvent>(Constants.Topics.NotifyPending);
        rider.AddProducer<NotifyDefaultDocumentsRequestEvent>(Constants.Topics.NotifyPendingDefaultDocuments);
        rider.AddProducer<RequireAllotmentRequestEvent>(Constants.Topics.NotifyAllotment);
    });
});

builder.Services.RegisterApplicationServices(builder.Configuration);
builder.Services.AddBmgMoneyMessaging();

builder.WebHost.UseUrls("http://0.0.0.0:5001");

builder.WebHost.UseSentry();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseAllElasticApm(builder.Configuration);

app.UseAuthorization();

app.MapControllers();

app.Run();
