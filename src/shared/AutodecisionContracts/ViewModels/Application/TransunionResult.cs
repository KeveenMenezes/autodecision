using System;

namespace AutodecisionCore.Contracts.ViewModels.Application;

public class TransunionResult
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime? BirthDate { get; set; }
    public string StreetAddress { get; set; }
    public string CityName { get; set; }
    public string StateName { get; set; }
    public string ZipCode { get; set; }
    public string SSN { get; set; }
    public string SessionId { get; set; }
    public int Score { get; set; }
    public string OfacHit { get; set; }
    public int DobScore { get; set; }
    public int AddressScore { get; set; }
    public int NameScore { get; set; }
    public int SSNScore { get; set; }
    public string RequestHash { get; set; }
}
