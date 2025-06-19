using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;

namespace OrchestratorAPI.Agents
{
    public class OrchestratorAgent
    {
        public ChatCompletionAgent GetAgent(Kernel kernel, List<ChatCompletionAgent> agents)
        {
            Kernel agentKernel = kernel.Clone();

            // Loop over agents and register them in the agent kernel
            if(agents != null && agents.Count > 0)
            {
                List<KernelFunction> subAgents = [];
                foreach (ChatCompletionAgent agent in agents)
                {
                    subAgents.Add(AgentKernelFunctionFactory.CreateFromAgent(agent));
                }

                KernelPlugin agentPlugin =KernelPluginFactory.CreateFromFunctions("AgentsPlugin", subAgents);
                agentKernel.Plugins.Add(agentPlugin);
            }
            agentKernel.FunctionInvocationFilters.Add(new FunctionCallFilter());

            return new()
            {
                Name = "orchestratoragent",
                Description = "This agent orchestrates the conversation between the user and the AI agents.",
                Instructions = """
                    You are an orchestrator agent that manages the conversation flow between different agents.
                    You will delegate tasks to other agents based on the user's input, consulting multiple agents if necessary.
                    Combine the results of the other agents but do not include the steps you took in between.
                    """,
                Kernel = agentKernel,
                Arguments = new KernelArguments(new PromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
            };

        }

        public sealed class FunctionCallFilter() : IFunctionInvocationFilter
        {
            public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
            {
                Console.WriteLine($"FunctionInvoking - {context.Function.PluginName}.{context.Function.Name}");

                await next(context);

                Console.WriteLine($"FunctionInvoked - {context.Function.PluginName}.{context.Function.Name}");
            }
        }
    }
}
