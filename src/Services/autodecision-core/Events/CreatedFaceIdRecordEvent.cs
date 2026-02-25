namespace AutodecisionCore.Events
{
    public class CreatedFaceIdRecordEvent
    {
        public int CustomerId { get; set; }
        public string FrontImagePath { get; set; }
        public string BackImagePath { get; set; }
        public bool IsEnrollmentNeeded { get; set; }
        public bool EnrollmentActive { get; set; }
        public bool IsDocumentScanNeeded { get; set; }
        public bool DocumentMatchActive { get; set; }
    }
}
