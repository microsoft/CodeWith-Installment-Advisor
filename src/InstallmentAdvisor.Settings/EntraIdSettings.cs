namespace InstallmentAdvisor.Settings;

public class EntraIdSettings : Base64SerializableSettings<EntraIdSettings>
{
    public const string Key = "AzureAd";

    public string ClientId { get; set; }

    public string TenantId { get; set; }

    public string Instance { get; set; }
}
