using Azure.AI.Agents.Persistent;
using InstallmentAdvisor.ChatApi.Models;
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
    public AzureAIAgent OrchestratorAgent { get; private set; } = null!;

    public AzureAIAgentThread GetOrCreateThread(string? threadId)
    {

        if (threadId == null)
        {
            var thread = new AzureAIAgentThread(OrchestratorAgent.Client);
            return thread;
        }
        else
        {
            return new AzureAIAgentThread(OrchestratorAgent.Client, threadId);
        }
    }

    public AzureAIAgent CreateOrchestratorAgentWithImageFilter(List<string> images)
    {
        AzureAIAgent agent = new(_persistentAgents[AgentConstants.ORCHESTRATOR_AGENT_NAME], _aiFoundryClient);
        RegisterSubAgents(agent, new List<Agent> { ScenarioAgent, VisualizationAgent, InstallmentRuleEvaluationAgent });
        agent.Kernel.FunctionInvocationFilters.Add(new ImageFilter(_aiFoundryClient, images));
        return agent;
    }

    private void Initialize(List<McpClientTool>? _tools)
    {
        ScenarioAgent = CreateAgent(_aiFoundryClient, _persistentAgents[AgentConstants.SCENARIO_AGENT_NAME], _tools);
        VisualizationAgent = CreateAgent(_aiFoundryClient, _persistentAgents[AgentConstants.VISUALIZATION_AGENT_NAME], _tools);
        InstallmentRuleEvaluationAgent = CreateAgent(_aiFoundryClient, _persistentAgents[AgentConstants.INSTALLMENT_RULE_EVALUATION_AGENT_NAME], _tools);
        OrchestratorAgent = CreateAgent(_aiFoundryClient, _persistentAgents[AgentConstants.ORCHESTRATOR_AGENT_NAME], new List<Agent> { ScenarioAgent, VisualizationAgent, InstallmentRuleEvaluationAgent }, null);
    }

    private AzureAIAgent CreateAgent(PersistentAgentsClient client, PersistentAgent agentDefinition, List<McpClientTool>? tools)
    {
        AzureAIAgent agent = new(agentDefinition, client);
        AddMcpTools(agent, tools);

        return agent;
    }

    private AzureAIAgent CreateAgent(PersistentAgentsClient client, PersistentAgent agentDefinition, List<Agent>? subAgents, List<ToolCall>? toolCallList)
    {
        AzureAIAgent agent = new(agentDefinition, client);

        RegisterSubAgents(agent, subAgents);
        if (toolCallList != null)
        {
            agent.Kernel.AutoFunctionInvocationFilters.Add(new AutoFunctionInvocationFilter(toolCallList));
        }

        return agent;
    }

    private void AddMcpTools(AzureAIAgent agent, List<McpClientTool>? tools)
    {
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

    private static void RegisterSubAgents(AzureAIAgent agent, List<Agent>? subAgents)
    {
        if (subAgents != null && subAgents.Count > 0)
        {
            List<KernelFunction> subAgentAsFunctions = new List<KernelFunction>();
            foreach (Agent subAgent in subAgents)
            {
                subAgentAsFunctions.Add(AgentKernelFunctionFactory.CreateFromAgent(subAgent));
            }
            KernelPlugin agentPlugin = KernelPluginFactory.CreateFromFunctions("AgentsPlugin", subAgentAsFunctions);
            agent.Kernel.Plugins.Add(agentPlugin);
        }
    }
}
