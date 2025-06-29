namespace InstallmentAdvisor.Settings;

public class CosmosDbSettings : Base64SerializableSettings<CosmosDbSettings>
{
    public const string Key = "CosmosDB";
    
    public string CosmosAccountEndpoint { get; set; }
    public string CosmosDatabaseName { get; set; }
    public string CosmosContainerName { get; set; }
}
