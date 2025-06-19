using Azure.AI.Agents.Persistent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.AzureAI;
using ModelContextProtocol.Client;
using OrchestratorAPI.Agents;

namespace OrchestratorAPI.Controllers
{
    [ApiController]
    [Route("chat")]
    public class ChatController : ControllerBase
    {

        private readonly ILogger<ChatController> _logger;
        private readonly Kernel _kernel;
        private readonly PersistentAgentsClient _aiFoundryClient;
        private readonly IMcpClient _mcpClient;

        public class ChatRequest
        {
            public required string Message { get; set; }
            public required string UserId { get; set; }
            public string? ThreadId { get; set; }
        }

        public ChatController(ILogger<ChatController> logger, Kernel kernel, PersistentAgentsClient aiFoundryClient, IMcpClient mcpClient)
        {
            _logger = logger;
            _kernel = kernel;
            _aiFoundryClient = aiFoundryClient;
            _mcpClient = mcpClient;
        }

        [HttpPost(Name = "chat")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest chatRequest)
        {

            // Create sub-agents.
            FoundryAgentFactory visualizationAgentFactory = new FoundryAgentFactory();
            ChatCompletionAgent scenarioAgent = await new ScenarioAgentFactory().GetAgentAsync(_kernel, _mcpClient);
            AzureAIAgent jokeAgent = await visualizationAgentFactory.GetAgentAsync(_aiFoundryClient);

            // Create orchestrator agent.
            ChatCompletionAgent orchestratorAgent = new OrchestratorAgentFactory().GetAgent(_kernel, [scenarioAgent, jokeAgent]);

            // Chat with orchestrator agent.
            AgentResponseItem<ChatMessageContent> chatResponse = await orchestratorAgent.InvokeAsync(chatRequest.Message).FirstAsync();

            // Delete 
            await visualizationAgentFactory.DeleteAgentAsync(_aiFoundryClient);
            
            // Create response object.
            var response = new
            {
                Message = chatResponse.Message.Content,
                ThreadId = chatResponse.Thread.Id,
            };

            // Return response string as json ok response.
            return Ok(response);

        }
    }
}
