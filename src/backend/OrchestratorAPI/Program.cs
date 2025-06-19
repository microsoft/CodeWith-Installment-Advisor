using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.AzureAI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddKernel().AddAzureOpenAIChatCompletion(
    deploymentName: builder.Configuration["modelName"]!,
    endpoint: builder.Configuration["baseUrl"]!,
    apiKey: builder.Configuration["apiKey"]!,
    apiVersion: "2025-01-01-preview");

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
