using Azure.AI.Agents.Persistent;
using Azure.Identity;
using InstallmentAdvisor.ChatApi;
using InstallmentAdvisor.ChatApi.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.Agents.AzureAI;

Console.WriteLine("Agent Provisioner starting...");

// Build configuration provider
IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>(optional: true)
    .Build();

// Use configuration to initialize other objects
string environmentName = configuration["EnvironmentName"] ?? "Production";
DefaultAzureCredentialOptions azureCredentialOptions = CredentialHelper.GetDefaultAzureCredentialOptions(environmentName);
var azureCredential = new DefaultAzureCredential(azureCredentialOptions);

string aiFoundryProjectEndpoint = configuration["AiFoundry:AiFoundryProjectEndpoint"]!;
PersistentAgentsClient aiFoundryClient = AzureAIAgent.CreateAgentsClient(aiFoundryProjectEndpoint, azureCredential);

string modelName = configuration["AiFoundry:ModelName"]!;

// Fetch current agents from Foundry
var existingAgents = aiFoundryClient.Administration.GetAgents();
var existingAgentsDict = existingAgents
    .GroupBy(a => a.Name)
    .ToDictionary(g => g.Key, g => g.First());

// Iterate through AgentConstants
var requiredAgentNames = new List<string>
{
    AgentConstants.JOKE_AGENT_NAME,
    AgentConstants.INSTALLMENT_RULE_EVALUATION_AGENT_NAME,
    AgentConstants.SCENARIO_AGENT_NAME,
    AgentConstants.ORCHESTRATOR_AGENT_NAME
};
foreach (var agentConstant in requiredAgentNames)
{
    // Get prompt from configuration
    string promptConfigKey = $"Agents:{agentConstant}:Prompt";
    string? configuredPrompt = configuration[promptConfigKey];

    bool hasCodeInterpreterTool = configuration.GetValue<bool>($"Agents:{agentConstant}:HasCodeInterpreterTool");

    if (string.IsNullOrWhiteSpace(configuredPrompt))
    {
        Console.WriteLine($"No prompt configured for agent '{agentConstant}', skipping.");
        continue;
    }

    if (existingAgentsDict.TryGetValue(agentConstant, out var existingAgent))
    {
        // Agent exists, check if prompt matches
        if (existingAgent.Instructions != configuredPrompt)
        {
            // Update agent with new prompt
            await aiFoundryClient.Administration.UpdateAgentAsync(existingAgent.Id, instructions: configuredPrompt);
            Console.WriteLine($"Updated agent '{agentConstant}' with new prompt.");
        }
        else
        {
            Console.WriteLine($"Agent '{agentConstant}' is up to date, utilize {existingAgent.Id}.");
        }
    }
    else
    {
        var tools = new List<ToolDefinition>();
        if(hasCodeInterpreterTool)
        {
            // Add code interpreter tool if configured
            tools.Add(new CodeInterpreterToolDefinition());
        }
        // Agent does not exist, create it
        var createdAgent = await aiFoundryClient.Administration.CreateAgentAsync(
            name: agentConstant,
            model: modelName,
            instructions: configuredPrompt,
            tools: tools
        );
        Console.WriteLine($"Created agent '{agentConstant}' with {createdAgent.Value.Id}.");
    }
}

Console.WriteLine("Agent Provisioner completed!");