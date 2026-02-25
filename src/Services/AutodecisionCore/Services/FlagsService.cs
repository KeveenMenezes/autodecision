using AutodecisionCore.Data.Models;
using AutodecisionCore.Data.Repositories.Interfaces;
using AutodecisionCore.Services.Interfaces;

namespace AutodecisionCore.Services
{
    public class FlagsService : IFlagsService
    {
        private readonly IFlagRepository _flagRepository;

        public FlagsService(IFlagRepository flagRepository)
        {
            _flagRepository = flagRepository;
        }

        public Task<List<Flag>> GetAllFlags()
        {
            return _flagRepository.GetAllFlagsAsync();  
        }
    }
}