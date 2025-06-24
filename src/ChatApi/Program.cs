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
    builder.Configuration["AiFoundry:ModelName"]!, 
    endpoint: builder.Configuration["AiFoundry:OpenAiBaseUrl"]!, 
    azureCredential
    );

// Inject foundry client for creating agents.
PersistentAgentsClient aiFoundryClient = AzureAIAgent.CreateAgentsClient(builder.Configuration["AiFoundry:AiFoundryProjectEndpoint"]!, azureCredential);
builder.Services.AddSingleton(aiFoundryClient);

// Inject mcp client.
List<McpClientTool> tools = new List<McpClientTool>();
try
{

    IMcpClient mcpClient = await McpClientFactory.CreateAsync(
        new SseClientTransport(
            new SseClientTransportOptions
            {
                Endpoint = new Uri(builder.Configuration["McpServer:McpServerEndpoint"]!),
                AdditionalHeaders = new Dictionary<string, string>
                {
                    { "Ocp-Apim-Subscription-Key", builder.Configuration["McpServer:McpServerApiKey"]! }                
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

    string accountEndpoint = builder.Configuration["CosmosDB:CosmosAccountEndpoint"]!;
    string databaseName = builder.Configuration["CosmosDB:CosmosDatabaseName"]!;
    string containerName = builder.Configuration["CosmosDB:CosmosContainerName"]!;

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

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

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
