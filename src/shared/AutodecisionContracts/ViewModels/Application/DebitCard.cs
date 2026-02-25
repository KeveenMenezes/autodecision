namespace AutodecisionCore.Contracts.ViewModels.Application;

public class DebitCard
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CardNumber { get; set; }
    public string CardBin { get; set; }
    public string Expiration { get; set; }
    public string CardBrand { get; set; }
    public string Vendor { get; set; }
    public bool Active { get; set; }
    public string CardBinEmissor { get; set; }
    public bool CardBinStatus { get; set; }
    public string BinName { get; set; }

    public bool IsConnected { get; set; }


}
