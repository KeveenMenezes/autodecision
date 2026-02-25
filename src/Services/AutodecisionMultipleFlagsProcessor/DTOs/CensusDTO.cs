namespace AutodecisionMultipleFlagsProcessor.DTOs
{
    public class CensusDTO
    {
        public CensusEmployer Data { get; set; }
        public string TransactionStatus { get; set; }
    }

    public class CensusEmployer
    {
        public int id { get; set; }
        public int? payroll_deduction_import_id { get; set; }
        public int employer_id { get; set; }
        public int calendar_id { get; set; }
        public int? customer_id { get; set; }
        public string employee_registration { get; set; }
        public string employee_registration_auxiliary { get; set; }
        public string position_number { get; set; }
        public string ssn { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string middle_name { get; set; }
        public string full_name { get; set; }
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string city_name { get; set; }
        public string state_abbreviation { get; set; }
        public string state_name { get; set; }
        public string phone_number { get; set; }
        public string zip_code { get; set; }
        public DateTime? date_of_birth { get; set; }
        public DateTime? hire_date { get; set; }
        public DateTime? original_hire_date { get; set; }
        public DateTime? termination_date { get; set; }
        public string job_title { get; set; }
        public string payment_method { get; set; }
        public string pay_cycle { get; set; }
        public int? periods_per_year { get; set; }
        public decimal? gross_annual_salary { get; set; }
        public decimal? salary_per_period { get; set; }
        public decimal? standard_hours { get; set; }
        public decimal? hourly_rate { get; set; }
        public string time_type { get; set; }
        public string payroll_group { get; set; }
        public string payroll_area { get; set; }
        public string personnel_sub_area { get; set; }
        public string personnel_sub_area_text { get; set; }
        public string eligible_for_benefits { get; set; }
        public string term_code { get; set; }
        public DateTime? term_date { get; set; }
        public string on_leave { get; set; }
        public string leave_of_absense_type { get; set; }
        public decimal? full_time_equivalent { get; set; }
        public string active_status { get; set; }
        public string census_status { get; set; }
        public DateTime created_at { get; set; }
        public string created_by { get; set; }
        public DateTime? updated_at { get; set; }
        public string updated_by { get; set; }
        public string salary_updated_by_fsal { get; set; }
    }
}
