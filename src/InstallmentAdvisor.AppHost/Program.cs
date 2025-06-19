var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.InstallmentAdvisor_DataApi>("data-api");
builder.AddProject<Projects.OrchestratorAPI>("orchestrator");

builder.Build().Run();
