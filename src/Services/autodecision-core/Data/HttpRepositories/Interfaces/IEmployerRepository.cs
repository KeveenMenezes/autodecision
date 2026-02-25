using AutodecisionCore.Contracts.ViewModels.Application;

namespace AutodecisionCore.Data.HttpRepositories.Interfaces
{
    public interface IEmployerRepository
    {
        Task<Employer> GetEmployerAsync(int employerId);
    }
}
