using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration.Handoff;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;

namespace OrchestratorAPI.Agents
{
    public class OrchestratorAgent
    {
        private readonly ChatCompletionAgent _agent;
        private readonly HandoffOrchestration _orchestration;
        private readonly ChatHistory history = [];

        public OrchestratorAgent(Kernel kernel)
        {
            Kernel agentKernel = kernel.Clone();
            _agent = new()
            {
                Name = "orchestratoragent",
                Description = "This agent orchestrates the conversation between the user and the AI agents.",
                Instructions = """
                    You are an orchestrator agent that manages the conversation flow between different agents.
                    You will delegate tasks to other agents based on the user's input, consulting multiple agents if necessary.
                    Combine the results of the other agents but do not include the steps you took in between.
                    """,
                Kernel = agentKernel,
            };

            // Initialize agents
            CapitalAgent capitalAgent = new(kernel);
            FoodAgent foodAgent = new(kernel);

            // Setup handoff orchestration
            var handoffs = OrchestrationHandoffs
                .StartWith(_agent)
                .Add(_agent, capitalAgent.Agent, "Transfer to this agent to get answers on capital related questions")
                .Add(_agent, foodAgent.Agent, "Transfer to this agent to get answers on food related questions")
                .Add(capitalAgent.Agent, _agent, "Transfer to this agent if the question is not about capital cities.")
                .Add(foodAgent.Agent, _agent, "Transfer to this agent if the question is not about food.");

            _orchestration = new HandoffOrchestration(
                handoffs,
                _agent,
                capitalAgent.Agent,
                foodAgent.Agent)
                {
                    ResponseCallback = ResponseCallbackFunction,
                    
                };
        }

        public async Task<object> ChatAsync(string input)
        {
            InProcessRuntime runtime = new();
            await runtime.StartAsync();
            var result = await _orchestration.InvokeAsync(input, runtime);
            string output = await result.GetValueAsync();
            var returnobject = new
            {
                response = output,
                steps = history
            };
            return returnobject;

        }

        ValueTask ResponseCallbackFunction(ChatMessageContent response)
        {
            history.Add(response);
            return ValueTask.CompletedTask;
        }
    }
}
