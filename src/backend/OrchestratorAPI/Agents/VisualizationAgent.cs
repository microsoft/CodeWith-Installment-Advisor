using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;

namespace OrchestratorAPI.Agents
{
    public class VisualizationAgent
    {
        public ChatCompletionAgent GetAgent(Kernel kernel)
        {
            Kernel agentKernel = kernel.Clone();
            return new ChatCompletionAgent
            {
                Name = "visualizationagent",
                Description = "This agent provides visual representations of energy consumption scenarios.",
                Instructions = """
                    You are a specialized agent that creates visualizations for energy consumption scenarios.
                    When asked about energy consumption, generate a visual representation of the scenario.
                    For example, if asked about energy consumption trends, create a chart or graph showing the trends over time.
                """,
                Kernel = agentKernel
            };
        }
    }
}
