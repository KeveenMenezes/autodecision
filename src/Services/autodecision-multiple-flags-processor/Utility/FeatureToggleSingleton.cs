using BmgMoney.FeatureToggle.DotNetCoreClient.Client;

namespace AutodecisionMultipleFlagsProcessor.Utility;
public class FeatureToggleSingleton
{
    private static FeatureToggleClient Instance;

    public FeatureToggleSingleton(string url, string environemnt)
    {
        if (Instance == null)
        {
            Console.WriteLine(@$"Instantiating Feature Toggle {url} - {environemnt}");
            Instance = new FeatureToggleClient(url, environemnt);
        }
    }

    public static FeatureToggleClient GetInstance()
    {
        return Instance;
    }
}
