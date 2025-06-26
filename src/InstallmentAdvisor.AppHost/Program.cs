using Aspire.Hosting;
using Microsoft.AspNetCore.Identity;

var builder = DistributedApplication.CreateBuilder(args);

var agentProvisioner = builder.AddProject<Projects.InstallmentAdvisor_FoundryAgentProvisioner>("agent-provisioner")
    .WithEnvironment("AiFoundry:ModelName", builder.Configuration["AiFoundry:ModelName"])
    .WithEnvironment("AiFoundry:OpenAiBaseUrl", builder.Configuration["AiFoundry:OpenAiBaseUrl"])
    .WithEnvironment("AiFoundry:AiFoundryProjectEndpoint", builder.Configuration["AiFoundry:AiFoundryProjectEndpoint"]);
var dataApi = builder.AddProject<Projects.InstallmentAdvisor_DataApi>("data-api");
var chatApi = builder.AddProject<Projects.InstallmentAdvisor_ChatApi>("chat-api")
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
    .WithEnvironment("Frontend:Url", builder.Configuration["Frontend:Url"])
    .WithEnvironment("agentIds", builder.Configuration["agentIds"])
    .WaitForCompletion(agentProvisioner);
var frontendApp = builder.AddNpmApp("frontend", "../frontend")
    .WithReference(chatApi)
    .WaitFor(chatApi)
    .WithHttpsEndpoint()
    .WithEnvironment("HTTPS", "true")
    .WithEnvironment("REACT_APP_CHAT_API", chatApi.GetEndpoint("https"))
    .WithExternalHttpEndpoints();

builder.Build().Run();


