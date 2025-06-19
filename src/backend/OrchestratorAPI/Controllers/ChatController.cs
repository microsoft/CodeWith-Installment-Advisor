using Azure.AI.Agents.Persistent;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.ChatCompletion;
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
        private readonly IHistoryRepository _historyRepository;

        public class ChatRequest
        {
            public required string Message { get; set; }
            public required string UserId { get; set; }
            public string? ThreadId { get; set; }
        }

        public ChatController(ILogger<ChatController> logger, Kernel kernel, PersistentAgentsClient aiFoundryClient, IMcpClient mcpClient, IHistoryRepository historyRepository)
        {
            _logger = logger;
            _kernel = kernel;
            _aiFoundryClient = aiFoundryClient;
            _mcpClient = mcpClient;
            _historyRepository = historyRepository;
        }

        [HttpPost(Name = "chat")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest chatRequest)
        {

            // Create sub-agents.
            FoundryAgentFactory visualizationAgentFactory = new();
            ChatCompletionAgent scenarioAgent = await ScenarioAgentFactory.CreateAgentAsync(_kernel, _mcpClient);
            AzureAIAgent jokeAgent = await visualizationAgentFactory.CreateAgentAsync(_aiFoundryClient);

            // Create orchestrator agent.
            ChatCompletionAgent orchestratorAgent = OrchestratorAgentFactory.CreateAgent(_kernel, [scenarioAgent, jokeAgent]);

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

            // Save chat history to repository.
            //await _historyRepository.AddMessageToHistoryAsync(chatRequest.UserId, chatResponse.Thread.Id!, chatRequest.Message, "user");
            //await _historyRepository.AddMessageToHistoryAsync(chatRequest.UserId, chatResponse.Thread.Id!, chatResponse.Message.Content!, "assistant");

            // Return response string as json ok response.
            return Ok(response);

        }
    }
}
