using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.AzureAI;
using ModelContextProtocol.Client;
using OrchestratorAPI.Helpers;

var builder = WebApplication.CreateBuilder(args);

DefaultAzureCredentialOptions azureCredentialOptions = CredentialHelper.GetDefaultAzureCredentialOptions(builder.Environment.EnvironmentName);
var azureCredential = new DefaultAzureCredential(azureCredentialOptions);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddKernel().AddAzureOpenAIChatCompletion(
    deploymentName: builder.Configuration["modelName"]!,
    endpoint: builder.Configuration["baseUrl"]!,
    apiKey: builder.Configuration["apiKey"]!,
    apiVersion: "2025-01-01-preview");


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


builder.Services.AddSingleton(mcpClient);


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
