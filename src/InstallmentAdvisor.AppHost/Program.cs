using Aspire.Hosting;
using Microsoft.AspNetCore.Identity;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.InstallmentAdvisor_DataApi>("data-api");
builder.AddProject<Projects.InstallmentAdvisor_ChatApi>("chat-api")
    .WithEnvironment("modelName", builder.Configuration["modelName"])
    .WithEnvironment("openAiBaseUrl", builder.Configuration["openAiBaseUrl"])
    .WithEnvironment("aiFoundryProjectEndpoint", builder.Configuration["aiFoundryProjectEndpoint"])
    .WithEnvironment("mcpServerEndpoint", builder.Configuration["mcpServerEndpoint"])
    .WithEnvironment("mcpServerApiKey", builder.Configuration["mcpServerApiKey"])
    .WithEnvironment("cosmosAccountEndpoint", builder.Configuration["cosmosAccountEndpoint"])
    .WithEnvironment("cosmosDatabaseName", builder.Configuration["cosmosDatabaseName"])
    .WithEnvironment("cosmosContainerName", builder.Configuration["cosmosContainerName"])
    .WithEnvironment("chatApiClientId", builder.Configuration["chatApiClientId"])
    .WithEnvironment("entraIdTenantId", builder.Configuration["entraIdTenantId"])
    .WithEnvironment("entraIdInstance", builder.Configuration["entraIdInstance"]);

builder.Build().Run();
