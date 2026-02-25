using AutodecisionCore.Contracts.Constants;
using AutodecisionCore.Core.AutoApproval.Factory.Interface;
using AutodecisionCore.Core.AutoApprovalCore.DTO;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;
using AutodecisionCore.Extensions;

namespace AutodecisionCore.Core.AutoApproval.Factory
{
	public class AutoApprovalItemFactory
	{
        public IAutoApprovalItemFactory GetFactory(AutoApprovalRequest request, ApplicationCore applicationCore)
        {
            if (request.Application.ProductId == ApplicationProductId.Cashless
                && request.Application.PaymentType == PayrollType.DebitCard)
                return new CashlessItemFactory();

            if (applicationCore.IsAutoApprovalFlagApproved()
                && !applicationCore.HasPendingApprovalFlagsBesides(new[] { FlagCode.LoanVerification, FlagCode.Flag209 }))
                return new RequiredItemFactory();

            return new DefaultItemFactory();
        }
    }
}
