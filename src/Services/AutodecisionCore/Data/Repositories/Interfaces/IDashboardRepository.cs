using AutodecisionCore.DTOs;

namespace AutodecisionCore.Data.Repositories.Interfaces
{
    public interface IDashboardRepository
    {

        Task<List<CountProcessingFlagDTO>> GetCountProcessingFlag(int timeUnit);
        Task<List<TimePeriodDTO>> GetTimePeriods();
        Task<List<AmountFlagsStatusDTO>> GetAmountFlagsByStatus(int timeUnit);
        Task<List<FlagWithErrorDTO>> GetFlagsWithError(int timeUnit);
        Task<TimePeriodDTO> GetTimePeriods(int? id);
        Task<List<ApplicationsProcessedByMinuteDTO>> GetApplicationProcessedByMinute(int timeUnit);
        Task<List<AmountApplicationByStatusDTO>> GetAmmountApplicationByStatus(int timeUnit);
        Task<List<AmountApplicationByStatusDetailDTO>> GetAmmountApplicationDetail(int timeUnit, int status);
    }
}
