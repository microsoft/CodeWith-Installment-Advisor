using Aspire.Hosting;
using Microsoft.AspNetCore.Identity;

var builder = DistributedApplication.CreateBuilder(args);

var agentProvisioner = builder.AddProject<Projects.InstallmentAdvisor_FoundryAgentProvisioner>("agent-provisioner");
builder.AddProject<Projects.InstallmentAdvisor_DataApi>("data-api");
builder.AddProject<Projects.InstallmentAdvisor_ChatApi>("chat-api")
    .WithEnvironment("AiFoundry:ModelName", builder.Configuration["AiFoundry:ModelName"])
    .WithEnvironment("AiFoundry:OpenAiBaseUrl", builder.Configuration["AiFoundry:OpenAiBaseUrl"])
    .WithEnvironment("AiFoundry:AiFoundryProjectEndpoint", builder.Configuration["AiFoundry:AiFoundryProjectEndpoint"])
    .WithEnvironment("McpServer:McpServerEndpoint", builder.Configuration["McpServer:McpServerEndpoint"])
    .WithEnvironment("McpServer:McpServerApiKey", builder.Configuration["McpServer:McpServerApiKey"])
    .WithEnvironment("CosmosDB:CosmosAccountEndpoint", builder.Configuration["CosmosDB:CosmosAccountEndpoint"])
    .WithEnvironment("CosmosDB:CosmosDatabaseName", builder.Configuration["CosmosDB:CosmosDatabaseName"])
    .WithEnvironment("CosmosDB:CosmosContainerName", builder.Configuration["CosmosDB:CosmosContainerName"])
    .WithEnvironment("AzureAd:ClientId", builder.Configuration["AzureAd:ClientId"])
    .WithEnvironment("AzureAd:TenantId", builder.Configuration["AzureAd:TenantId"])
    .WithEnvironment("AzureAd:Instance", builder.Configuration["AzureAd:Instance"])
    .WaitForCompletion(agentProvisioner);

builder.Build().Run();


