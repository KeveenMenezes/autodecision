using AutodecisionCore.Data.Models;

namespace AutodecisionCore.Services.Interfaces
{
    public interface IFlagsService
    {
        Task<List<Flag>> GetAllFlags();
    }
}