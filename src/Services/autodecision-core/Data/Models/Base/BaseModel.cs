using AutodecisionCore.Utils;

namespace AutodecisionCore.Data.Models.Base
{
    public class BaseModel
    {
        public BaseModel()
        {
            CreatedAt = DateTimeUtil.Now;
            IsDeleted ??= false;
        }

        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
