using Azure.AI.Agents.Persistent;
using Azure.Core;
using Azure.Identity;
using Microsoft.Identity.Web;
using InstallmentAdvisor.ChatApi.Agents;
using InstallmentAdvisor.ChatApi.Helpers;
using InstallmentAdvisor.ChatApi.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.AzureAI;
using ModelContextProtocol.Client;
using InstallmentAdvisor.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

DefaultAzureCredentialOptions azureCredentialOptions = CredentialHelper.GetDefaultAzureCredentialOptions(builder.Environment.EnvironmentName);
var azureCredential = new DefaultAzureCredential(azureCredentialOptions);

// Add CORS services for Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(builder.Configuration["Frontend:Url"]!)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();

builder.Services.AddOpenApi();


var aiFoundrySettings = AiFoundrySettings.FromBase64String(builder.Configuration[AiFoundrySettings.Key]!);
builder.Services.AddKernel().AddAzureOpenAIChatCompletion(
    aiFoundrySettings.ModelName, 
    endpoint: aiFoundrySettings.AiFoundryProjectEndpoint, 
    azureCredential
    );

// Inject foundry client for creating agents.
PersistentAgentsClient aiFoundryClient = AzureAIAgent.CreateAgentsClient(aiFoundrySettings.AiFoundryProjectEndpoint, azureCredential);
builder.Services.AddSingleton(aiFoundryClient);

// Inject mcp client.
List<McpClientTool> tools = new List<McpClientTool>();
try
{
    var mcpServerSettings = McpServerSettings.FromBase64String(builder.Configuration[McpServerSettings.Key]!);
    //var mcpToken = await azureCredential.GetTokenAsync(
    //    new TokenRequestContext(
    //        new[] { mcpServerSettings.ApiId }
    //    )
    //);
    var loggerFactory = LoggerFactory.Create(builder =>
    {
        builder.AddConsole(c =>
        {
            c.LogToStandardErrorThreshold = LogLevel.Trace;
        });
    });

    var mcpClient = await McpClientFactory.CreateAsync(
        new SseClientTransport(
            new SseClientTransportOptions
            {
                Endpoint = new Uri(mcpServerSettings.McpServerEndpoint),
                AdditionalHeaders = new Dictionary<string, string>
                {
                    { "Ocp-Apim-Subscription-Key", mcpServerSettings.McpServerApiKey },
                    //{ "Authorization", $"Bearer {mcpToken.Token}" }

                }
            }
        ), loggerFactory: loggerFactory
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
    var cosmosSettings = CosmosDbSettings.FromBase64String(builder.Configuration[CosmosDbSettings.Key]);

    // Create and configure CosmosClientOptions
    var cosmosClientOptions = new CosmosClientOptions
    {
        ConnectionMode = ConnectionMode.Direct,
        RequestTimeout = TimeSpan.FromSeconds(30)
    };
    var client = new CosmosClient(cosmosSettings.CosmosAccountEndpoint, azureCredential, cosmosClientOptions);
    var database = client.GetDatabase(cosmosSettings.CosmosDatabaseName);
    return database.GetContainer(cosmosSettings.CosmosContainerName);
});
builder.Services.AddSingleton<IHistoryRepository, CosmosHistoryRepository>();

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

var agentIds = builder.Configuration["agentIds"]?.Split(';') ?? Array.Empty<string>();
var agentService = new AgentService(aiFoundryClient, agentIds, tools, builder.Configuration);
builder.Services.AddSingleton(agentService);

var app = builder.Build();

app.MapDefaultEndpoints();

// Use CORS before any endpoint mapping
app.UseCors("AllowFrontend");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapControllers();

app.Run();
