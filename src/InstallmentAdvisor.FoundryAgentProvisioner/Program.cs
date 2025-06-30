using Azure.AI.Agents.Persistent;
using Azure.Identity;
using InstallmentAdvisor.ChatApi;
using InstallmentAdvisor.ChatApi.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.Agents.AzureAI;
using InstallmentAdvisor.Settings;

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

AiFoundrySettings aiFoundrySettings = AiFoundrySettings.FromBase64String(configuration[AiFoundrySettings.Key]!);

PersistentAgentsClient aiFoundryClient = AzureAIAgent.CreateAgentsClient(aiFoundrySettings.AiFoundryProjectEndpoint, azureCredential);

AgentsSettings agentsSettings = AgentsSettings.FromBase64String(configuration[AgentsSettings.Key]!);

// Fetch current agents from Foundry
var existingAgents = aiFoundryClient.Administration.GetAgents();
var existingAgentsDict = existingAgents
    .GroupBy(a => a.Name)
    .ToDictionary(g => g.Key, g => g.First());

// Iterate through AgentConstants
var requiredAgentNames = new List<string>
{
    AgentConstants.VISUALIZATION_AGENT_NAME,
    AgentConstants.INSTALLMENT_RULE_EVALUATION_AGENT_NAME,
    AgentConstants.SCENARIO_AGENT_NAME,
    AgentConstants.ORCHESTRATOR_AGENT_NAME,
    AgentConstants.UPDATE_INSTALLMENT_AMOUNT_AGENT_NAME
};
foreach (var agentConfiguration in agentsSettings.Agents)
{
    string? configuredPrompt = agentConfiguration.Prompt;

    bool hasCodeInterpreterTool = agentConfiguration.HasCodeInterpreterTool;

    if (string.IsNullOrWhiteSpace(agentConfiguration.Prompt))
    {
        Console.WriteLine($"No prompt configured for agent '{agentConfiguration.AgentName}', skipping.");
        continue;
    }

    var tools = new List<ToolDefinition>();
    if (hasCodeInterpreterTool)
    {
        // Add code interpreter tool if configured
        tools.Add(new CodeInterpreterToolDefinition());
    }

    if (existingAgentsDict.TryGetValue(agentConfiguration.AgentName, out var existingAgent))
    {
        // Agent exists, check if prompt matches
        if (ShouldUpdateAgent(existingAgent, agentConfiguration, tools))
        {
            // Update agent with new prompt
            await aiFoundryClient.Administration.UpdateAgentAsync(existingAgent.Id, 
                instructions: agentConfiguration.Prompt, 
                model: agentConfiguration.ModelName, 
                tools: tools, 
                temperature: agentConfiguration.Temperature, 
                topP: agentConfiguration.TopP
            );
            Console.WriteLine($"Updated agent '{agentConfiguration.AgentName}' with new prompt.");
        }
        else
        {
            Console.WriteLine($"Agent '{agentConfiguration.AgentName}' is up to date, utilize {existingAgent.Id}.");
        }
    }
    else
    {
        // Agent does not exist, create it
        var createdAgent = await aiFoundryClient.Administration.CreateAgentAsync(
            name: agentConfiguration.AgentName,
            model: agentConfiguration.ModelName,
            instructions: configuredPrompt,
            tools: tools,
            temperature: agentConfiguration.Temperature,
            topP: agentConfiguration.TopP
        );
        Console.WriteLine($"Created agent '{agentConfiguration.AgentName}' with {createdAgent.Value.Id}.");
    }
}

Console.WriteLine("Agent Provisioner completed!");

static bool ShouldUpdateAgent(dynamic existingAgent, AgentConfigurationSettings agentConfiguration, List<ToolDefinition> tools)
{
    if (existingAgent.Instructions != agentConfiguration.Prompt)
        return true;
    if (existingAgent.Model != agentConfiguration.ModelName)
        return true;
    if (existingAgent.Temperature != agentConfiguration.Temperature)
        return true;
    if (existingAgent.TopP != agentConfiguration.TopP)
        return true;

    if (existingAgent.Tools is IEnumerable<ToolDefinition> existingTools)
    {
        if (existingTools.Count() != tools.Count)
            return true;
        var existingTypes = existingTools.Select(t => t.GetType()).OrderBy(t => t.FullName).ToList();
        var newTypes = tools.Select(t => t.GetType()).OrderBy(t => t.FullName).ToList();
        if (!existingTypes.SequenceEqual(newTypes))
            return true;
    }

    return false;
}
