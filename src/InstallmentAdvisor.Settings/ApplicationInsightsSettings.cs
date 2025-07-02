namespace InstallmentAdvisor.Settings;

public class ApplicationInsightsSettings : Base64SerializableSettings<AgentsSettings>
{
    public const string Key = "ApplicationInsights";

    public string ConnectionString { get; set; } = "";
}
