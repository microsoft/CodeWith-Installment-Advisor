var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.InstallmentAdvisor_DataApi>("data-api");
builder.AddProject<Projects.InstallmentAdvisor_ChatApi>("chat-api");

builder.Build().Run();
