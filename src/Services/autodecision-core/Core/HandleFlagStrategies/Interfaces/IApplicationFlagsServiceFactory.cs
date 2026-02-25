using AutodecisionCore.Services.Interfaces;

namespace AutodecisionCore.Core.HandleFlagStrategies.Interfaces
{
    public interface IApplicationFlagsServiceFactory
    {
        IApplicationFlagsService GetService();
    }
}
