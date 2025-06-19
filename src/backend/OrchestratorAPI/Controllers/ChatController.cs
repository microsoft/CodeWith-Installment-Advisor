using Azure.AI.Agents.Persistent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using OrchestratorAPI.Agents;

namespace OrchestratorAPI.Controllers
{
    [ApiController]
    [Route("chat")]
    public class ChatController : ControllerBase
    {

        private readonly ILogger<ChatController> _logger;
        private readonly Kernel _kernel;

        public class ChatRequest
        {
            public required string Message { get; set; }
            public required string UserId { get; set; }
            public string? ThreadId { get; set; }
        }

        public ChatController(ILogger<ChatController> logger, Kernel kernel)
        {
            _logger = logger;
            _kernel = kernel;
        }

        [HttpPost(Name = "chat")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest chatRequest)
        {

            // Create sub-agents.
            ChatCompletionAgent scenarioAgent = new ScenarioAgent().GetAgent(_kernel);
            ChatCompletionAgent visualizationAgent = new VisualizationAgent().GetAgent(_kernel);

            // Create orchestrator agent.
            ChatCompletionAgent orchestratorAgent = new OrchestratorAgent().GetAgent(_kernel, [scenarioAgent, visualizationAgent]);

            // Chat with orchestrator agent.
            AgentResponseItem<ChatMessageContent> chatResponse = await orchestratorAgent.InvokeAsync(chatRequest.Message).FirstAsync();

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
