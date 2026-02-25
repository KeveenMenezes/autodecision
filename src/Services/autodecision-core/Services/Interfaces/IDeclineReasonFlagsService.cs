using AutodecisionCore.Data.Models;

namespace AutodecisionCore.Services.Interfaces
{
    public interface IDeclineReasonFlagsService
    {
        Task<List<DeclineReasonFlags>> GetAllActiveAsync();
    }
}
