using AutodecisionCore.Data.Models.Trigger;

namespace AutodecisionCore.Services.Interfaces
{
    public interface ITriggerService
    {
        Task<List<string>> GetTriggerFlagCodesByProcessingReasonAsync(string reason);
    }
}
