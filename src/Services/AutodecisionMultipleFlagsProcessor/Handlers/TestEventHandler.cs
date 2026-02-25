using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Services;
using BMGMoney.SDK.V2.Cache.Redis;
using MassTransit;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class TestEventHandler : IConsumer<ProcessFlagRequestEvent>
    {

        private readonly ILogger<TestEventHandler> _logger;
        private readonly IFlagHelper _flagHelper;

        public TestEventHandler(ILogger<TestEventHandler> logger, IFlagHelper flagHelper)
        {
            _logger = logger;
            _flagHelper = flagHelper;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

			// fazer a lógica da Flag...
			var response = ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;

			// publicar a resposta...
			await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var r = new ProcessFlagResponseEvent();

            try 
            { 
                if (autodecisionCompositeData.BlockList != null)
                    r.FlagResult = AutodecisionCore.Contracts.Enums.FlagResultEnum.Processed;

                return r;
            }
            catch (Exception e)
            {
                r.Message = e.Message;
                r.FlagResult = FlagResultEnum.Error;
                return r;
            }
        }

    }
}
