using AutodecisionCore.DTOs;

namespace AutodecisionCore.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<List<AverageTimeProcessingFlagDTO>> GetAverageTimeProcessingFlags(int? timePeriodId);
        Task<List<TimePeriodDTO>> GetTimePeriods();
        Task<List<AmountFlagsStatusDTO>> GetAmountFlagsByStatus(int? timePeriodId);
        Task<List<FlagWithErrorDTO>> GetFlagsWithError(int? timePeriodId);
        Task<List<TimeProcessingApplicationResponseDTO>> GetProcessingApplicationTime(int? id);
        Task<AmountApplicatonByStatusResponseDTO> GetAmountApplicationByStatus(int? timePeriodId);
        Task<List<AmountApplicationByStatusDetailDTO>> GetAmountApplicationByStatusDetails(int? timePeriodId, int status);
    }
}
