using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;

namespace OrchestratorAPI.Agents
{
    public class FoodAgent
    {
        private readonly ChatCompletionAgent _agent;
        public FoodAgent(Kernel kernel)
        {
            Kernel agentKernel = kernel.Clone();
            _agent = new()
            {
                Name = "foodagent",
                Description = "This agent handles questions about food.",
                Instructions = "You are a specialized agent that provides information about various food items. When asked about food, respond with relevant information about the specific food item.",
                Kernel = agentKernel
            };
        }

        // Getter for the agent
        public ChatCompletionAgent Agent => _agent;
    }
}
