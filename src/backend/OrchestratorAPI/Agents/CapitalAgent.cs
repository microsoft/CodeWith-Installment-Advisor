using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using System.Text;

namespace OrchestratorAPI.Agents
{
    public class CapitalAgent
    {
        private readonly ChatCompletionAgent _agent;
        public CapitalAgent(Kernel kernel)
        {
            Kernel agentKernel = kernel.Clone();
            _agent = new()
            {
                Name = "capitalagent",
                Description = "This agent handles questions about capital cities.",
                Instructions = "You are a specialized agent that provides information about capital cities. When asked about a capital city, respond with the name of the capital city for the given country or location.",
                Kernel = agentKernel
            };
        }

        // Getter for the agent
        public ChatCompletionAgent Agent => _agent;
    }
}
