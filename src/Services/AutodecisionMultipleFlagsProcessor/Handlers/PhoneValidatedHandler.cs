using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Services;
using MassTransit;


namespace AutodecisionMultipleFlagsProcessor.Handlers;


public class PhoneValidatedHandler : IConsumer<ProcessFlagRequestEvent>
{
    private readonly ILogger<PhoneValidatedHandler> _logger;
    private readonly IFlagHelper _flagHelper;
    private const string FlagID = FlagCode.PhoneValidated;

    public PhoneValidatedHandler(ILogger<PhoneValidatedHandler> logger,
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
        var response = new ProcessFlagResponseEvent();
        try
        {
            response = _flagHelper.BuildFlagResponse(FlagCode.PhoneValidated, autodecisionCompositeData, FlagResultEnum.Processed);

            if (autodecisionCompositeData.Customer.PhoneValidated != 1)
            {
                _logger.LogInformation($"PhoneValidated flag raised for customerID: {autodecisionCompositeData.Customer.Id}");
                response.FlagResult = FlagResultEnum.PendingApproval;
                response.Message = "The customer did not validated the phone number.";
                _flagHelper.RaiseFlag(response, "The customer did not validated the phone number.");
                return response;
            }

            _logger.LogInformation($"Phone already validated for customerID: {autodecisionCompositeData.Customer.Id}");
            response.FlagResult = FlagResultEnum.Approved;
            response.Message = "Phone already validated for customerID.";
            return response;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error processing PhoneValidated flag for customerID: {autodecisionCompositeData.Customer.Id}");
            response.FlagResult = FlagResultEnum.Error;
            response.Message = "Error processing PhoneValidated flag.";
            return response;
        }
    }
}