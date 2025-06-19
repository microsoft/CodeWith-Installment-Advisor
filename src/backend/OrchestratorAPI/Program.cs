using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.AzureAI;
using ModelContextProtocol.Client;
using OrchestratorAPI.Helpers;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

DefaultAzureCredentialOptions azureCredentialOptions = CredentialHelper.GetDefaultAzureCredentialOptions(builder.Environment.EnvironmentName);
var azureCredential = new DefaultAzureCredential(azureCredentialOptions);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddKernel().AddAzureOpenAIChatCompletion(builder.Configuration["modelName"]!, endpoint: builder.Configuration["openAiBaseUrl"]!,azureCredential);

// Inject foundry client for creating agents.
PersistentAgentsClient aiFoundryClient = AzureAIAgent.CreateAgentsClient(builder.Configuration["aiFoundryProjectEndpoint"]!, azureCredential);
builder.Services.AddSingleton(aiFoundryClient);

// Inject mcp client.
var mcpClient = await McpClientFactory.CreateAsync(
    new SseClientTransport(
        new SseClientTransportOptions
        {
            Endpoint = new Uri(builder.Configuration["mcpServerEndpoint"]!),
            AdditionalHeaders = new Dictionary<string, string>
            {
                { "Ocp-Apim-Subscription-Key", builder.Configuration["mcpServerApiKey"]! }
            }
        }
    )
);

var tools = await mcpClient.ListToolsAsync().ConfigureAwait(false);

builder.Services.AddSingleton(tools.ToList());

// Inject cosmos history repository.
builder.Services.AddSingleton(sp =>
{
    string accountEndpoint = builder.Configuration["cosmosAccountEndpoint"]!;
    string databaseName = builder.Configuration["cosmosDatabaseName"]!;
    string containerName = builder.Configuration["cosmosContainerName"]!;

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
