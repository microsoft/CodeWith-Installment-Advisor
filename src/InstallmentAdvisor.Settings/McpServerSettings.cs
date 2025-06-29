namespace InstallmentAdvisor.Settings;

public class McpServerSettings : Base64SerializableSettings<McpServerSettings>
{
    public const string Key = "McpServer";

    public string ApiId { get; set; }

    public string McpServerApiKey { get; set; }

    public string McpServerEndpoint { get; set; }
}
