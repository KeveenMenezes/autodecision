using AutodecisionCore.Data.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutodecisionCore.Data.Models
{
    public class Flag: BaseModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        public bool InternalFlag { get; set; }
		public bool IsWarning { get; set; }

		public Flag(string code, string name, string description)
        {
            Code = code;
            Name = name;
            Description = description;
            Active = true;
        }

        public Flag(string code, string name, string description, bool internalFlag, bool isWarning)
        {
            Code = code;
            Name = name;
            Description = description;
            Active = true;
            InternalFlag = internalFlag;
            IsWarning = isWarning;
        }
    }
}
