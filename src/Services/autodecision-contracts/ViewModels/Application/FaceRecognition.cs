using System.Collections.Generic;

namespace AutodecisionCore.Contracts.ViewModels.Application;

public class FaceRecognition
{
    public string EnrollmentStatus { get; set; }

    public bool? Liveness { get; set; }

    public bool? DocumentScanSuccess { get; set; }

    public string FraudStatus { get; set; }

    public List<int> ClientIdsMatch { get; set; }
    public DocumentDataScanned DocumentData { get; set; }

}

public class DocumentDataScanned
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string DateOfBirth { get; set; }

    public string Address { get; set; }

    public string Sex { get; set; }

    public string IdNumber { get; set; }
}
