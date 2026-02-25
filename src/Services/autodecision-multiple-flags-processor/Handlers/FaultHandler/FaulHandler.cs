using AutodecisionCore.Contracts.Messages;
using AutodecisionMultipleFlagsProcessor.Services;
using MassTransit;
using System.Text.Json;

namespace AutodecisionMultipleFlagsProcessor.Handlers.FaultHandler
{
    public class FaulHandler : IConsumer<Fault<ProcessFlagRequestEvent>>
    {
        private readonly ILogger<FaulHandler> _logger;

        public FaulHandler(ILogger<FaulHandler> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<Fault<ProcessFlagRequestEvent>> context)
        {
            var faultDetails = new FaultDetails
            {
                Message = JsonSerializer.Serialize(context.Message.Message)
            };

            _logger.LogWarning("Error when trying to consume message", faultDetails);

            return Task.CompletedTask;
        }

        internal class FaultDetails
        {
            public string Message { get; set; }
            public string Source { get; set; }
        }
    }
}
