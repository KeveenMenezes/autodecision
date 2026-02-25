namespace AutodecisionCore.Data.HttpRepositories.DTOs
{
    public class EmployerDTO
    {
        public int id { get; set; }
        public string name { get; set; }
        public string program { get; set; }
        public int? sub_program_id { get; set; }
        public int? due_diligence_status { get; set; }
    }
}
