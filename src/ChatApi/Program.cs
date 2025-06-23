using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Identity.Web;
using InstallmentAdvisor.ChatApi.Helpers;
using InstallmentAdvisor.ChatApi.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.AzureAI;
using ModelContextProtocol.Client;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

DefaultAzureCredentialOptions azureCredentialOptions = CredentialHelper.GetDefaultAzureCredentialOptions(builder.Environment.EnvironmentName);
var azureCredential = new DefaultAzureCredential(azureCredentialOptions);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddKernel().AddAzureOpenAIChatCompletion(
    Environment.GetEnvironmentVariable("modelName")!, 
    endpoint: Environment.GetEnvironmentVariable("openAiBaseUrl")!, 
    azureCredential
    );

// Inject foundry client for creating agents.
PersistentAgentsClient aiFoundryClient = AzureAIAgent.CreateAgentsClient(Environment.GetEnvironmentVariable("aiFoundryProjectEndpoint")!, azureCredential);
builder.Services.AddSingleton(aiFoundryClient);

// Inject mcp client.
List<McpClientTool> tools = new List<McpClientTool>();
try
{

    IMcpClient mcpClient = await McpClientFactory.CreateAsync(
        new SseClientTransport(
            new SseClientTransportOptions
            {
                Endpoint = new Uri(Environment.GetEnvironmentVariable("mcpServerEndpoint")!),
                AdditionalHeaders = new Dictionary<string, string>
                {
                    { "Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable("mcpServerApiKey")! }                
                }
            }
        )
    );

    var toolResponse = await mcpClient.ListToolsAsync().ConfigureAwait(false);
    tools = [.. toolResponse];

}
catch (Exception ex)
{
    // Log the error (replace with your logger if available)
    Console.Error.WriteLine($"Failed to create MCP client: {ex.Message}");
}

builder.Services.AddSingleton(tools);


// Inject cosmos history repository.
builder.Services.AddSingleton(sp =>
{
    string accountEndpoint = Environment.GetEnvironmentVariable("cosmosAccountEndpoint")!;
    string databaseName = Environment.GetEnvironmentVariable("cosmosDatabaseName")!;
    string containerName = Environment.GetEnvironmentVariable("cosmosContainerName")!;

    // Create and configure CosmosClientOptions
    var cosmosClientOptions = new CosmosClientOptions
    {
        ConnectionMode = ConnectionMode.Direct,
        RequestTimeout = TimeSpan.FromSeconds(30)
    };
    var client = new CosmosClient(accountEndpoint, azureCredential, cosmosClientOptions);
    var database = client.GetDatabase(databaseName);
    return database.GetContainer(containerName);
});
builder.Services.AddSingleton<IHistoryRepository, CosmosHistoryRepository>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddMicrosoftIdentityWebApi(bearerTokenOptions => { }, identityOptions =>
{
    identityOptions.ClientId = Environment.GetEnvironmentVariable("chatApiClientId")!;
    identityOptions.TenantId = Environment.GetEnvironmentVariable("entraIdTenantId")!;
    identityOptions.Instance = Environment.GetEnvironmentVariable("entraIdInstance")!;
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.UseAuthorization();

app.MapControllers();

app.Run();
