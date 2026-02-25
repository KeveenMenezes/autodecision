using AutodecisionCore.Contracts.ViewModels.Application;

namespace AutodecisionCore.Data.HttpRepositories.Interfaces
{
    public interface IFacetecRepository
    {
        Task<FaceRecognition> GetFaceRecognition(int customerId);
    }
}
