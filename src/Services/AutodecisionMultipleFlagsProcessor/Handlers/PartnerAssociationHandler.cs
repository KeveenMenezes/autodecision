using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Services;
using MassTransit;

namespace AutodecisionMultipleFlagsProcessor.Handlers
{
    public class PartnerAssociationHandler : IConsumer<ProcessFlagRequestEvent>
    {
        private readonly ILogger<PartnerAssociationHandler> _logger;
        private readonly IFlagHelper _flagHelper;
        private const string FlagID = FlagCode.PartnerAssociation;

        public PartnerAssociationHandler(ILogger<PartnerAssociationHandler> logger,
                                         IFlagHelper flagHelper)
        {
            _logger = logger;
            _flagHelper = flagHelper;
        }

        public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
        {
            var autodecisionComposite = await _flagHelper.GetAutodecisionCompositeData(context.Message.Key);

            if (!_flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagID, _logger))
                return;

			var response = ProcessFlag(autodecisionComposite);
			response.Reason = context.Message.Reason;

			await _flagHelper.SendReponseMessage(response);
        }

        public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData autodecisionCompositeData)
        {
            var response = _flagHelper.BuildFlagResponse(FlagID, autodecisionCompositeData);

            try
            {
                var program = ApplicationProgram.LoansForAll;
                if (autodecisionCompositeData.Application.Program != program)
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    response.Message = $"Application program is not program {program}.";

                    return response;
                }

                if (!autodecisionCompositeData.Application.EmployerIsAssociation)
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    response.Message = $"The employer {autodecisionCompositeData.Application.EmployerId} does not have an association.";

                    return response;
                }

                if (autodecisionCompositeData.Application.PartnerId != autodecisionCompositeData.Application.EmployerPartnerId)
                {
                    response.FlagResult = FlagResultEnum.Ignored;
                    response.Message = $"The application does not have the employer partner." +
                                       $"Application PartnerId: {autodecisionCompositeData.Application.PartnerId} | Employer PartnerId: {autodecisionCompositeData.Application.EmployerPartnerId}.";
                    
                    return response;
                }

                response.FlagResult = FlagResultEnum.PendingApproval;
                response.Message = $"Customer is from a partner association with {autodecisionCompositeData.Application.PartnerName}.";

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"FlagCode: {FlagID} was not successfully processed for LoanNumber: {autodecisionCompositeData.Application.LoanNumber} | Error: {e.Message}");

                response.Message = e.Message;
                response.FlagResult = FlagResultEnum.Error;

                return response;
            }
        }
    }
}
