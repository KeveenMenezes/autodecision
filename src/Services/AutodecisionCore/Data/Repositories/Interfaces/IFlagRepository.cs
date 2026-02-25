using AutodecisionCore.Data.Models;
using AutodecisionCore.Data.Repositories.Base;

namespace AutodecisionCore.Data.Repositories.Interfaces
{
    public interface IFlagRepository : IBaseRepository<Flag>
    {
        Task<List<Flag>> GetAllActiveFlagsAsync();
        Task<List<Flag>> GetAllActiveInternalFlagsAsync();

        Task<Flag> GetFlagByCodeAsync(string code);
        Task<List<Flag>> GetAllFlagsAsync();

        Task<List<Flag>> GetAllActiveFlagsExceptWarningAsync();

        Task<List<Flag>> GetAllActiveInternalFlagsExceptWarningAsync();

	}
}
