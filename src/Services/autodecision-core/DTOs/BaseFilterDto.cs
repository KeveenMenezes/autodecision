namespace AutodecisionCore.DTOs
{
    public class BaseFilterDto
    {
        public BaseFilterDto() { }

        public BaseFilterDto(
            int? id = null,
            string? description = null,
            DateTime? createAtStart = null,
            DateTime? createAtEnd = null,
            bool? isDeleted = null)
        {
            Id = id;
            Description = description;
            CreateAtStart = createAtStart;
            CreateAtEnd = createAtEnd;
            IsDeleted = isDeleted;
        }

        public int? Id { get; set; }
        public string? Description { get; set; }
        public DateTime? CreateAtStart { get; set; }
        public DateTime? CreateAtEnd { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
