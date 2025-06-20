using Domain;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;

namespace OrchestratorAPI.Agents
{
    public class OrchestratorAgentFactory
    {
        public static ChatCompletionAgent CreateAgent(Kernel kernel, List<Agent> agents, List<ToolCall>? toolCallList)
        {
            Kernel agentKernel = kernel.Clone();

            // Loop over agents and register them in the agent kernel
            if(agents != null && agents.Count > 0)
            {
                List<KernelFunction> subAgents = new List<KernelFunction>();
                foreach (Agent agent in agents)
                {
                    subAgents.Add(AgentKernelFunctionFactory.CreateFromAgent(agent));
                }

                KernelPlugin agentPlugin =KernelPluginFactory.CreateFromFunctions("AgentsPlugin", subAgents);
                agentKernel.Plugins.Add(agentPlugin);
            }

            if(toolCallList != null)
            {
                agentKernel.AutoFunctionInvocationFilters.Add(new AutoFunctionInvocationFilter(toolCallList));
            }

            return new()
            {
                Name = "orchestratoragent",
                Description = "This agent orchestrates the conversation between the user and the AI agents.",
                Instructions = """
                    You are an orchestrator agent that manages the conversation flow between different agents.
                    You will delegate tasks to other agents based on the user's input, consulting multiple agents if necessary.
                    If the user asks questions about energy usage, installment amounts etc., use the scenario agent to provide detailed information about energy consumption scenarios.
                    If the user asks for a joke, use the joke agent to provide a humorous energy-related joke.
                    """,
                Kernel = agentKernel,
                Arguments = new KernelArguments(new PromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
            };

        }

        public class AutoFunctionInvocationFilter(List<ToolCall> toolCallList) : IAutoFunctionInvocationFilter
        {
            public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
            {
                await next(context);
                var parametersList = new List<ToolParameter>();

                if (context.Arguments != null)
                {
                    parametersList = context.Arguments.Select(p => new ToolParameter { Key = p.Key, Value = p.Value?.ToString() }).ToList();
                }
                var response = context.Result.GetValue<ChatMessageContent[]>()?.First();
                ToolCall toolCallInfo = new ToolCall
                {
                    FunctionName = context.Function.Name,
                    PluginName = context.Function.PluginName!,
                    Parameters = parametersList,
                    Response = response?.Content
                };

                toolCallList.Add(toolCallInfo);
            }
        }
    }
}
