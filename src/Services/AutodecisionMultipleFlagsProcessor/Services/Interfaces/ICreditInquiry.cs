namespace AutodecisionMultipleFlagsProcessor.Services.Interfaces
{
    public interface ICreditInquiry
    {
        Task GetIdVerification(int customerId);
        string CheckTransunionBankruptcy(string loanNumber);
        string CheckPacerBankruptcy(string loanNumber);

    }
}
