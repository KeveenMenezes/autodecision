using AutodecisionCore.Contracts.ViewModels.Application;

namespace AutodecisionCore.Data.HttpRepositories.Interfaces
{
    public interface ICustomerRepository
    {
        Task<Customer> GetCustomerInfo(int customerId);
    }
}
