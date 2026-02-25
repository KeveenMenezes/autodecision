using AutodecisionCore.Data.Repositories.Interfaces;
using AutodecisionCore.Services.Interfaces;

namespace AutodecisionCore.Services
{
    public class TriggerService : ITriggerService
    {
        private readonly ITriggerRepository _triggerRepository;

        public TriggerService(ITriggerRepository triggerRepository)
        {
            _triggerRepository = triggerRepository;
        }

        public async Task<List<string>> GetTriggerFlagCodesByProcessingReasonAsync(string reason)
        {
            var trigger = await _triggerRepository.FindByDescriptionIncludeTriggerFlagsAsync(reason);

            return trigger != null ? trigger.Flags.Select(flag => flag.FlagCode).ToList() : new List<string>();
        }
    }
}