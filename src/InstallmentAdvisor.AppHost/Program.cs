using Aspire.Hosting;
using InstallmentAdvisor.Settings;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var agentSettings = new AgentsSettings();
builder.Configuration.GetSection(AgentsSettings.Key).Bind(agentSettings);

var aiFoundrySettings = new AiFoundrySettings();
builder.Configuration.GetSection(AiFoundrySettings.Key).Bind(aiFoundrySettings);

var mcpServerSettings = new McpServerSettings();
builder.Configuration.GetSection(McpServerSettings.Key).Bind(mcpServerSettings);

var cosmosDbSettings = new CosmosDbSettings();
builder.Configuration.GetSection(CosmosDbSettings.Key).Bind(cosmosDbSettings);

var entraIdSettings = new EntraIdSettings();
builder.Configuration.GetSection(EntraIdSettings.Key).Bind(entraIdSettings);

var applicationInsightsSettings = new ApplicationInsightsSettings();
builder.Configuration.GetSection(ApplicationInsightsSettings.Key).Bind(applicationInsightsSettings);

var azureAiSearchSettings = new AzureAiSearchSettings();
builder.Configuration.GetSection(AzureAiSearchSettings.Key).Bind(azureAiSearchSettings);

var agentProvisioner = builder.AddProject<Projects.InstallmentAdvisor_FoundryAgentProvisioner>("agent-provisioner")
    .WithEnvironment(AgentsSettings.Key, agentSettings.ToBase64String())
    .WithEnvironment(AiFoundrySettings.Key, aiFoundrySettings.ToBase64String())
    .WithEnvironment("EnvironmentName", builder.Environment.EnvironmentName);

var dataApi = builder.AddProject<Projects.InstallmentAdvisor_DataApi>("data-api")
    .WithEnvironment(AzureAiSearchSettings.Key, azureAiSearchSettings.ToBase64String())
    .WithEnvironment(AiFoundrySettings.Key, aiFoundrySettings.ToBase64String());

var chatApi = builder.AddProject<Projects.InstallmentAdvisor_ChatApi>("chat-api")
    .WithEnvironment(AiFoundrySettings.Key, aiFoundrySettings.ToBase64String())
    .WithEnvironment(McpServerSettings.Key, mcpServerSettings.ToBase64String())
    .WithEnvironment(AgentsSettings.Key, agentSettings.ToBase64String())
    .WithEnvironment(CosmosDbSettings.Key, cosmosDbSettings.ToBase64String())
    .WithEnvironment(EntraIdSettings.Key, entraIdSettings.ToBase64String())
    .WithEnvironment("Frontend:Url", builder.Configuration["Frontend:Url"])
    .WithEnvironment("agentIds", builder.Configuration["agentIds"])
    .WithEnvironment("APPLICATIONINSIGHTS_CONNECTION_STRING", applicationInsightsSettings.ConnectionString)
    .WithEnvironment("EnvironmentName", builder.Environment.EnvironmentName)
    .WaitForCompletion(agentProvisioner);

var frontendApp = builder.AddNpmApp("frontend", "../frontend")
    .WithReference(chatApi)
    .WaitFor(chatApi)
    .WithEnvironment("HTTPS", "true")
    .WithEnvironment("REACT_APP_CHAT_API", chatApi.GetEndpoint("https"))
    .WithExternalHttpEndpoints();

builder.Build().Run();

