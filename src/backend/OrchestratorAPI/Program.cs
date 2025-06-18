using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

// Semantic kernel
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
