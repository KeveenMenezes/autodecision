using AutodecisionCore.Data.Models;
using AutodecisionCore.Data.Repositories.Base;

namespace AutodecisionCore.Data.Repositories.Interfaces
{
    public interface IDeclineReasonFlagsRepository : IBaseRepository<DeclineReasonFlags>
    {
        Task<List<DeclineReasonFlags>> GetAllActiveAsync();
    }
}
