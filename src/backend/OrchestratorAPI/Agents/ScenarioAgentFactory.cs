using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using ModelContextProtocol.Client;

namespace OrchestratorAPI.Agents
{
    public class ScenarioAgentFactory
    {
        public async Task<ChatCompletionAgent> GetAgentAsync(Kernel kernel, IMcpClient? mcpClient)
        {
            Kernel agentKernel = kernel.Clone();

            if(mcpClient != null)
            {
                var tools = await mcpClient.ListToolsAsync().ConfigureAwait(false);
                agentKernel.Plugins.AddFromFunctions("MCP", tools.Select(tool => tool.AsKernelFunction()));
            }

            return new ChatCompletionAgent
            {
                Name = "scenarioagent",
                Description = "This agent handles questions about energy consumption scenarios like installment amounts.",
                Instructions = """
                    You are a specialized agent that provides information about energy consumption scenarios.
                    When asked about energy consumption, usage or installment amounts, respond with relevant information about the specific scenario.
                    For example, if asked about installment amounts, provide the calculated installment amount based on the given parameters.
                """,
                Kernel = agentKernel,
                Arguments = new KernelArguments(new PromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })

            };
        }
    }
}
