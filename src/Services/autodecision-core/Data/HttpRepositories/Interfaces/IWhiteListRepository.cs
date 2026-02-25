using AutodecisionCore.Contracts.ViewModels.Application;

namespace AutodecisionCore.Data.HttpRepositories.Interfaces
{
    public interface IWhiteListRepository
    {
        Task<WhiteList?> GetWhiteListInfo(int customerId);
    }
}
