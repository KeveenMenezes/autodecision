using AutodecisionMultipleFlagsProcessor.DTOs;

namespace AutodecisionMultipleFlagsProcessor.Services.Interfaces
{
    public interface IOpenBankingService
    {
        Task<DailyReceivingsDTO> GetDailyReceivings(int customerId);
    }
}
