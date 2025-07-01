using Azure.AI.Agents.Persistent;
using InstallmentAdvisor.Settings;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.AzureAI;
using ModelContextProtocol.Client;

namespace InstallmentAdvisor.ChatApi.Agents;

public class AgentService
{
    private readonly PersistentAgentsClient _aiFoundryClient;
    private Dictionary<string, PersistentAgent> _persistentAgents;
    private AgentsSettings _configuration;

    public AgentService(PersistentAgentsClient aiFoundryClient, string[] agentIds, List<McpClientTool>? _tools, AgentsSettings configuration)
    {
        _aiFoundryClient = aiFoundryClient;
        _persistentAgents = new Dictionary<string, PersistentAgent>();
        _configuration = configuration;

        foreach (var agentId in agentIds)
        {
            var agent = _aiFoundryClient.Administration.GetAgent(agentId);

            if (agent.Value != null)
            {
                _persistentAgents.Add(agent.Value.Name, agent.Value);
            }
        }

        Initialize(_tools);
    }

    public AzureAIAgent ScenarioAgent { get; private set; } = null!;
    public AzureAIAgent VisualizationAgent { get; private set; } = null!;
    public AzureAIAgent InstallmentRuleEvaluationAgent { get; private set; } = null!;
    public AzureAIAgent UpdateInstallmentAmountAgent { get; set; }

    public async Task<AzureAIAgentThread> GetOrCreateThreadAsync(string? threadId)
    {

        if (threadId == null)
        {
            // Create thread manually to ensure it is created with threadId.
            var aiFoundryThread = await _aiFoundryClient.Threads.CreateThreadAsync();
            var thread = new AzureAIAgentThread(_aiFoundryClient, aiFoundryThread.Value.Id);
            return thread;
        }
        else
        {
            return new AzureAIAgentThread(_aiFoundryClient, threadId);
        }
    }

    public ChatCompletionAgent CreateOrchestratorAgent(Kernel kernel, List<string> images, AzureAIAgentThread aiAgentThread)
    {
        var settings = GetSettingsForAgent("orchestrator-agent");
        Kernel agentKernel = kernel.Clone();

        List<KernelFunction> subAgents = new List<KernelFunction>();
        RegisterSubAgents(agentKernel, aiAgentThread, new List<Agent> { ScenarioAgent, VisualizationAgent, InstallmentRuleEvaluationAgent, UpdateInstallmentAmountAgent });
        agentKernel.FunctionInvocationFilters.Add(new ImageFilter(_aiFoundryClient, images));
        
        return new()
        {
            Name = "orchestratoragent",
            Description = "This agent orchestrates the conversation between the user and the AI agents.",
            Instructions = settings.Prompt,
            Kernel = agentKernel,
            Arguments = new KernelArguments(new PromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),  })
        };
    }

    private void Initialize(List<McpClientTool>? _tools)
    {
        ScenarioAgent = CreateAgent(_aiFoundryClient, _persistentAgents[AgentConstants.SCENARIO_AGENT_NAME], _tools);
        VisualizationAgent = CreateAgent(_aiFoundryClient, _persistentAgents[AgentConstants.VISUALIZATION_AGENT_NAME], _tools);
        InstallmentRuleEvaluationAgent = CreateAgent(_aiFoundryClient, _persistentAgents[AgentConstants.INSTALLMENT_RULE_EVALUATION_AGENT_NAME], _tools);
        UpdateInstallmentAmountAgent = CreateAgent(_aiFoundryClient, _persistentAgents[AgentConstants.UPDATE_INSTALLMENT_AMOUNT_AGENT_NAME], _tools, new List<Agent> { InstallmentRuleEvaluationAgent });
    }

    private AzureAIAgent CreateAgent(PersistentAgentsClient client, PersistentAgent agentDefinition, List<McpClientTool>? tools)
    {
        AzureAIAgent agent = new(agentDefinition, client);
        AddMcpTools(agent, tools);

        return agent;
    }

    private void AddMcpTools(AzureAIAgent agent, List<McpClientTool>? tools)
    {
        if(agent.Name == null)
        {
            return;
        }
        var agentConfiguration = GetSettingsForAgent(agent.Name);
        var toolsFound = agentConfiguration.AvailableMcpTools;
        if (toolsFound != null && toolsFound.Count > 0 && tools != null && tools.Count > 0)
        {
            var selectedTools = tools
                .Where(tool => toolsFound.Contains(tool.Name))
                .Select(tool => tool.AsKernelFunction())
                .ToList();

            if (selectedTools.Count > 0)
            {
                agent.Kernel.Plugins.AddFromFunctions("MCP", selectedTools);
            }
        }
        else if (tools != null && tools.Count > 0)
        {
            agent.Kernel.Plugins.AddFromFunctions("MCP", tools.Select(tool => tool.AsKernelFunction()));
        }
    }

    private AgentConfigurationSettings GetSettingsForAgent(string agentName)
    {
        return _configuration.Agents.Single(a => a.AgentName == agentName);
    }

    private static void RegisterSubAgents(Kernel kernel, AzureAIAgentThread aiAgentThread, List<Agent>? subAgents)
    {
        if (subAgents != null && subAgents.Count > 0)
        {
            List<KernelFunction> subAgentAsFunctions = new List<KernelFunction>();
            foreach (Agent subAgent in subAgents)
            {
                subAgentAsFunctions.Add(AgentAsToolFactory.CreateFromAgent(subAgent, aiAgentThread));
            }
            KernelPlugin agentPlugin = KernelPluginFactory.CreateFromFunctions("AgentsPlugin", subAgentAsFunctions);
            kernel.Plugins.Add(agentPlugin);
        }
    }
}
