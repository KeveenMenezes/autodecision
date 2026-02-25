using AutodecisionCore.Contracts.ViewModels.Application;

namespace AutodecisionCore.DTOs;

public class NewOpenBanking
{
    public List<AccountsResponse> ActiveAccounts { get; set; }
    public bool NotAutoPrefill { get; set; }
    public int VendorId { get; set; }
}
