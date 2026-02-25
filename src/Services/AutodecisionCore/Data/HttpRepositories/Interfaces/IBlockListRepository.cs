using AutodecisionCore.Contracts.ViewModels.Application;

namespace AutodecisionCore.Data.HttpRepositories.Interfaces
{
    public interface IBlockListRepository
    {
        Task<BlockList?> GetBlockListInfo(int customerId);
    }
}
