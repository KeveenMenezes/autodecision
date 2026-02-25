using AutodecisionCore.Data.Models;
using AutodecisionCore.Data.Repositories.Interfaces;
using AutodecisionCore.Services.Interfaces;

namespace AutodecisionCore.Services
{
    public class DeclineReasonFlagsService : IDeclineReasonFlagsService
    {
        private readonly IDeclineReasonFlagsRepository _declineReasonFlagsRepository;

        public DeclineReasonFlagsService(IDeclineReasonFlagsRepository declineReasonFlagsRepository) 
        {
            _declineReasonFlagsRepository = declineReasonFlagsRepository;
        }

        public async Task<List<DeclineReasonFlags>> GetAllActiveAsync()
        {
            return await _declineReasonFlagsRepository.GetAllActiveAsync();
        }
    }
}
