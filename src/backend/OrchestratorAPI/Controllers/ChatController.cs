using Azure.AI.Agents.Persistent;
using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.ChatCompletion;
using ModelContextProtocol.Client;
using OpenAI.Assistants;
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
        private readonly List<McpClientTool> _tools;
        private readonly IHistoryRepository _historyRepository;

        public class ChatRequest
        {
            public required string Message { get; set; }
            public required string UserId { get; set; }
            public string? ThreadId { get; set; }
        }

        public ChatController(ILogger<ChatController> logger, Kernel kernel, PersistentAgentsClient aiFoundryClient, List<McpClientTool> tools, IHistoryRepository historyRepository)
        {
            _logger = logger;
            _kernel = kernel;
            _aiFoundryClient = aiFoundryClient;
            _tools = tools;
            _historyRepository = historyRepository;
        }

        [HttpPost(Name = "chat")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest chatRequest)
        {

            ChatHistoryAgentThread agentThread = new ChatHistoryAgentThread();
            // If threadId is provided, retrieve chat history.
            if (!string.IsNullOrEmpty(chatRequest.ThreadId))
            {
                List<ChatMessage> chatHistory = await _historyRepository.GetHistoryAsync(chatRequest.UserId, chatRequest.ThreadId);
                if (chatHistory.Count > 0)
                {
                    agentThread = buildAgentThread(chatHistory);
                }
            }

            // Create sub-agents.
            ChatCompletionAgent scenarioAgent = ScenarioAgentFactory.CreateAgent(_kernel, _tools);
            AzureAIAgent jokeAgent = await FoundryAgentFactory.CreateAgentAsync(_aiFoundryClient);

            // Create orchestrator agent.
            ChatCompletionAgent orchestratorAgent = OrchestratorAgentFactory.CreateAgent(_kernel, [scenarioAgent, jokeAgent]);

            // Chat with orchestrator agent.
            AgentResponseItem<ChatMessageContent> chatResponse = await orchestratorAgent.InvokeAsync(chatRequest.Message, agentThread).FirstAsync();

            // Delete agent.
            await FoundryAgentFactory.DeleteAgentAsync(_aiFoundryClient, jokeAgent.Id);
            

            // Create response object.
            var response = new
            {
                Message = chatResponse.Message.Content,
                ThreadId = chatResponse.Thread.Id,
            };

            // Save chat history to repository.
            await _historyRepository.AddMessageToHistoryAsync(chatRequest.UserId, chatResponse.Thread.Id!, chatRequest.Message, "user");
            await _historyRepository.AddMessageToHistoryAsync(chatRequest.UserId, chatResponse.Thread.Id!, chatResponse.Message.Content!, "assistant");

            // Return response string as json ok response.
            return Ok(response);

        }

        [HttpDelete("/chat/{threadId}")]
        public async Task<IActionResult> DeleteChat([FromRoute] string threadId, [FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(threadId) || string.IsNullOrEmpty(userId))
            {
                return BadRequest("ThreadId and UserId are required.");
            }
            // Delete chat history from repository.
            bool deleted = await _historyRepository.DeleteHistoryAsync(userId, threadId);

            if (deleted)
            {
                return Ok();
            }
            else
            {
                return NotFound("Chat history not found for the provided ThreadId and UserId.");
            }

        }

        private ChatHistoryAgentThread buildAgentThread (List<ChatMessage> messages)
        {
            ChatHistory history = new ChatHistory();
            foreach (ChatMessage message in messages)
            {
                if (message.Role == "user")
                {
                    history.AddUserMessage(message.Content);
                }
                else if (message.Role == "assistant")
                {
                    history.AddAssistantMessage(message.Content);
                }
                else if (message.Role == "system")
                {
                    history.AddSystemMessage(message.Content);
                }
            }
            return new ChatHistoryAgentThread(history);
        }
    }
}
