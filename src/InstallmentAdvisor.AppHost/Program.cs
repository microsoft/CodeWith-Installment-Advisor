var builder = DistributedApplication.CreateBuilder(args);

var agentProvisioner = builder.AddProject<Projects.InstallmentAdvisor_FoundryAgentProvisioner>("agent-provisioner");
builder.AddProject<Projects.InstallmentAdvisor_DataApi>("data-api");
builder.AddProject<Projects.InstallmentAdvisor_ChatApi>("chat-api")
    .WaitForCompletion(agentProvisioner);

builder.Build().Run();
