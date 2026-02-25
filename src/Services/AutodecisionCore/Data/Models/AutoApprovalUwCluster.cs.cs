using AutodecisionCore.Data.Models.Base;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutodecisionCore.Data.Models
{
	public class AutoApprovalUwCluster : BaseModel
	{
		[Column(TypeName = "varchar(16)")]
		public string UwCluster { get; set; }
		public bool IsAllowed { get; set; }
	}
}
