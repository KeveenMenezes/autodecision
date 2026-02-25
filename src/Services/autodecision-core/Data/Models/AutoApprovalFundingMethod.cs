using AutodecisionCore.Data.Models.Base;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutodecisionCore.Data.Models
{
	public class AutoApprovalFundingMethod : BaseModel
	{
		[Column(TypeName = "varchar(16)")]
		public string FundingMethod { get; set; }
		public bool IsAllowed { get; set; }
    }
}
