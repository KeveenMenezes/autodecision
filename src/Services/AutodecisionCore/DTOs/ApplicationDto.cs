using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;

namespace AutodecisionCore.DTOs
{
    public class ApplicationDto
    {
        public string LoanNumber { get; set; }
        public List<ApplicationFlagDto> ApplicationFlags { get; set; } = new List<ApplicationFlagDto>();


    }
}
