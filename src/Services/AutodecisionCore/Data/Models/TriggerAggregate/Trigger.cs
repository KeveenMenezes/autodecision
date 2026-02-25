using AutodecisionCore.Data.Models.Base; 

namespace AutodecisionCore.Data.Models.Trigger
{
    public class Trigger : BaseModel
    {
        public string Description { get; private set; }
        public virtual ICollection<TriggerFlag> Flags { get; private set; } = new List<TriggerFlag>();
    }
}
