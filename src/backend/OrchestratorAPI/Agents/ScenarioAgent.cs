using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;

namespace OrchestratorAPI.Agents
{
    public class ScenarioAgent
    {
        public ChatCompletionAgent GetAgent(Kernel kernel)
        {
            Kernel agentKernel = kernel.Clone();
            return new ChatCompletionAgent
            {
                Name = "scenarioagent",
                Description = "This agent handles questions about energy consumption scenarios like installment amounts.",
                Instructions = """
                    You are a specialized agent that provides information about energy consumption scenarios.
                    When asked about energy consumption, respond with relevant information about the specific scenario.
                    For example, if asked about installment amounts, provide the calculated installment amount based on the given parameters.
                """,
                Kernel = agentKernel
            };
        }
    }
}
