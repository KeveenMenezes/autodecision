using AutodecisionCore.Data.Models.Base;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutodecisionCore.Data.Models
{
	public class AutoApprovalPaymentType : BaseModel
	{
		[Column(TypeName = "varchar(64)")]
		public string PaymentType { get; set; }
		public bool IsAllowed { get; set; }
	}
}
