using System;

namespace AutodecisionCore.Contracts.ViewModels.Application;

public class Customer
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Ssn { get; set; }
    public string PhoneNumber { get; set; }
    public int? PhoneValidated { get; set; }
    public string SecondaryPhoneNumber { get; set; }
    public string WorkPhoneNumber { get; set; }
    public decimal? VerifiedNetIncome { get; set; }
    public DateTime? VerifiedNetIncomeDate { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string StreetAddress { get; set; }
    public string CityName { get; set; }
    public string StateName { get; set; }
    public string ZipCode { get; set; }
    public string StateAbbreviation { get; set; }
    public string BankName { get; set; }
    public decimal? Salary { get; set; }
    public string? PlaidToken { get; set; }
    public string Email { get; set; }
    public string MobileNetworkType { get; set; }
    public string UnitNumber { get; set; }
    public string EmployeeRegistration { get; set; }
    public DateTime? VerifiedDateOfHire { get; set; }

    public Customer()
    {

    }
}
