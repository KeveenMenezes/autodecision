using System.Globalization;
using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Contracts.Enums;
using AutodecisionCore.Contracts.Messages;
using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.Extensions;
using AutodecisionMultipleFlagsProcessor.Services;
using BmgMoney.FeatureToggle.DotNetCoreClient.Interface;
using MassTransit;

namespace AutodecisionMultipleFlagsProcessor.Handlers;

public class LevelOfCommitmentHandler(
    ILogger<LevelOfCommitmentHandler> logger,
    IFlagHelper flagHelper,
    IFeatureToggleClient featureToggleClient)
    : IConsumer<ProcessFlagRequestEvent>
{
    public async Task Consume(ConsumeContext<ProcessFlagRequestEvent> context)
    {
        var autodecisionComposite = await flagHelper.GetAutodecisionCompositeData(context.Message.Key);

        if (!flagHelper.IsValidVersion(context.Message, autodecisionComposite, FlagCode.LevelOfCommitment, logger))
            return;

        var response = ProcessFlag(autodecisionComposite);
        response.Reason = context.Message.Reason;

        await flagHelper.SendReponseMessage(response);
    }

    public ProcessFlagResponseEvent ProcessFlag(AutodecisionCompositeData compositeData)
    {
        var response = flagHelper.BuildFlagResponse(FlagCode.LevelOfCommitment, compositeData, FlagResultEnum.AutoDeny);

        try
        {
            var informedIncome = GetIncomeInformed(compositeData);

            if (IsIgnored(response, compositeData.Application.Program, compositeData.FlagValidatorHelper.EmployerAllowAutoDeny))
                return response;

            if (IsPendingApproval(response, compositeData, informedIncome))
                return response;

            IsProcessed(response, compositeData, informedIncome);           
                     
            return response;
        }
        catch (Exception e)
        {
            logger.LogError(e, "FlagCode: {LevelOfCommitment} was not successfully processed for LoanNumber: {LoanNumber} | Error: {Message}", FlagCode.LevelOfCommitment, compositeData.Application.LoanNumber, e.Message);

            response.Message = e.Message;
            response.FlagResult = FlagResultEnum.Error;
            return response;
        }
    }

    private bool IsIgnored(ProcessFlagResponseEvent response, string program, bool employerAllowAutoDeny)
    {
        var IsIgnored = false;
        if (!featureToggleClient.IsEnabled("Flags253And255"))
        {
            response.FlagResult = FlagResultEnum.Ignored;
            response.Message += "Flag ignored due to feature toggle settings.";

            IsIgnored = true;
        }
        if (IsLAWProgram(program))
        {
            response.FlagResult = FlagResultEnum.Ignored;
            response.Message += "Flag ignored due to Loans At Work program.";

            IsIgnored = true;
        }
        if (!employerAllowAutoDeny)
        {
            response.FlagResult = FlagResultEnum.Ignored;
            response.Message += "Flag ignored due to employer not allowing auto deny.";

            IsIgnored = true;
        }

        return IsIgnored;
    }

    private static bool IsPendingApproval(ProcessFlagResponseEvent response, AutodecisionCompositeData compositeData, decimal? informedIncome)
    {
        var isPending = false;
        if (!ValidNetIncome(informedIncome))
        {
            response.FlagResult = FlagResultEnum.PendingApproval;
            response.Message += "NetIncome invalid. Value cannot be null or zero.";

            isPending = true;
        }
        if (!ValidAmountOfPayment(compositeData.Application.AmountOfPayment, GetOldAmountOfPayment(compositeData)))
        {
            response.FlagResult = FlagResultEnum.PendingApproval;
            response.Message += "Amount of payment less than old payment from last application.";

            isPending = true;
        }
        if (HasSeveralEmployers(compositeData))
        {
            response.FlagResult = FlagResultEnum.PendingApproval;
            response.Message += "There is more than one employer on payout to this customer.";

            isPending = true;
        }

        return isPending;
    }

    private static void IsProcessed(ProcessFlagResponseEvent response, AutodecisionCompositeData compositeData, decimal? informedIncome)
    {       
        var applicationLOC = CalculateApplicationLOC(compositeData, informedIncome);
        var creditPolicyLOC = GetCreditPolicyLOC(compositeData);

        if (HasLOCValid(creditPolicyLOC, applicationLOC))
        {
            response.FlagResult = FlagResultEnum.Processed;
            response.Message = "Flag processed successfully.";           
        }
        else
        {
            response.FlagResult = FlagResultEnum.AutoDeny;         
            response.Message = $"Application denied by income. Application LOC: {(applicationLOC * 100).ToString("F2", CultureInfo.InvariantCulture)}% | Credit Policy LOC: {(creditPolicyLOC * 100)?.ToString("F2", CultureInfo.InvariantCulture)}%";
        }                              
    }

    private static bool IsLAWProgram(string program) =>
        program == BMGMoneyProgram.LoansAtWork;

    private static bool HasSeveralEmployers(AutodecisionCompositeData compositeData) =>
        compositeData.OpenPayroll.Connections.Any(x => x.HasMoreThanOneEmployer.Equals(true));

    private static bool ValidNetIncome(decimal? netIncome) => !(netIncome == 0 || netIncome is null);

    private static bool HasLOCValid(decimal? creditPolicyLOC, decimal? applicationLOC) => (applicationLOC <= creditPolicyLOC);

    private static bool ValidAmountOfPayment(decimal amountOfPayment, decimal? oldAmountOfPayment) =>
        amountOfPayment > (oldAmountOfPayment ?? 0);

    private static decimal? GetCreditPolicyLOC(AutodecisionCompositeData compositeData) =>
        compositeData.Application.Type == ApplicationType.NewLoan ? compositeData.CreditPolicy.CreditPolicyEntity.LocNew : compositeData.CreditPolicy.CreditPolicyEntity.LocRefi;

    private static decimal CalculateApplicationLOC(AutodecisionCompositeData compositeData, decimal? netIncome) =>
        compositeData.Application.VerifiedNetIncome > 0 ? compositeData.Application.AmountOfPayment / (netIncome ?? 0) : 0;

    private static decimal? GetOldAmountOfPayment(AutodecisionCompositeData compositeData) =>
        compositeData?.LastApplications?.FirstOrDefault(x => x.Id == compositeData.Application?.PreviousApplicationId)?.AmountOfPayment;

    private static decimal? GetIncomeInformed(AutodecisionCompositeData compositeData) =>
        compositeData?.TotalIncome?.TotalAmount;
}
