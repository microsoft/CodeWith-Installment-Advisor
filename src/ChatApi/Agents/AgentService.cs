using Azure.AI.Agents.Persistent;
using InstallmentAdvisor.ChatApi.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.AzureAI;
using ModelContextProtocol.Client;

namespace InstallmentAdvisor.ChatApi.Agents;

public class AgentService
{
    private readonly PersistentAgentsClient _aiFoundryClient;
    private Dictionary<string, PersistentAgent> _persistentAgents;

    public AgentService(PersistentAgentsClient aiFoundryClient, string[] agentIds, List<McpClientTool>? _tools)
    {
        _aiFoundryClient = aiFoundryClient;
        _persistentAgents = new Dictionary<string, PersistentAgent>();

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
    public AzureAIAgent JokeAgent { get; private set; } = null!;
    public AzureAIAgent InstallmentRuleEvaluationAgent { get; private set; } = null!;
    public AzureAIAgent OrchestratorAgent { get; private set; } = null!;

    public AzureAIAgentThread GetOrCreateThread(string? threadId)
    {

        if (threadId == null)
        {
            return new AzureAIAgentThread(OrchestratorAgent.Client);
        }
        else
        {
            return new AzureAIAgentThread(OrchestratorAgent.Client, threadId);
        }
    }

    private void Initialize(List<McpClientTool>? _tools)
    {
        ScenarioAgent = CreateAgent(_aiFoundryClient, _persistentAgents[AgentConstants.SCENARIO_AGENT_NAME], _tools);
        JokeAgent = CreateAgent(_aiFoundryClient, _persistentAgents[AgentConstants.JOKE_AGENT_NAME], _tools);
        InstallmentRuleEvaluationAgent = CreateAgent(_aiFoundryClient, _persistentAgents[AgentConstants.INSTALLMENT_RULE_EVALUATION_AGENT_NAME], _tools);
        OrchestratorAgent = CreateAgent(_aiFoundryClient, _persistentAgents[AgentConstants.ORCHESTRATOR_AGENT_NAME], new List<Agent> { ScenarioAgent, JokeAgent, InstallmentRuleEvaluationAgent }, null);
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
        if (tools != null && tools.Count > 0)
        {
            agent.Kernel.Plugins.AddFromFunctions("MCP", tools.Select(tool => tool.AsKernelFunction()));
        }
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
