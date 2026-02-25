using AutodecisionCore.Contracts.ViewModels.Application;
using AutodecisionMultipleFlagsProcessor.DTOs;

namespace AutodecisionMultipleFlagsProcessor.Data.Repositories.Interfaces
{
    public interface IHouseholdHitRepository
    {
        Task<List<SimilarAddressDto>> GetSimilarAddressListAsync(Customer customer, decimal max_similarity);
    }
}