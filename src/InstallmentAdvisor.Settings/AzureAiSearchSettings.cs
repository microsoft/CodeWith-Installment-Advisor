namespace InstallmentAdvisor.Settings;
public class AzureAiSearchSettings : Base64SerializableSettings<AzureAiSearchSettings>
{
    public const string Key = "AzureAISearch";

    public string ServiceName { get; set; }
    
    public string IndexName { get; set; }

    public string ApiKey { get; set; }

    public string Endpoint { get; set; }

    public string EmbeddingDeploymentName { get; set; }

    public string Version { get; set; } = "2023-07-01";
}
