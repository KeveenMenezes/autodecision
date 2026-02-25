using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionCore.Data.Models.AutodecisionCoreAggregate;

namespace AutodecisionCore.Services.Interfaces
{
    public interface IAutodecisionCompositeService
    {
        Task<AutodecisionCompositeData> FillCompositeData(Application application, ApplicationCore applicationCore);
        Task<AutodecisionCompositeData> GetCompositeDataFromRedis(string key);
    }
}
