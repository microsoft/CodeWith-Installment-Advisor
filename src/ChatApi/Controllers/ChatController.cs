using Azure.AI.Agents.Persistent;
using InstallmentAdvisor.ChatApi.Agents;
using InstallmentAdvisor.ChatApi.Repositories;
using InstallmentAdvisor.ChatApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.ChatCompletion;
using ModelContextProtocol.Client;
using System.Dynamic;
using System.Text;


namespace InstallmentAdvisor.ChatApi.Controllers
{
    [ApiController]
    [Route("chat")]
    public class ChatController : ControllerBase
    {
        private readonly AgentService _agentService;
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
            public bool Stream { get; set; }
            public bool? Debug { get; set; }
        }

        public ChatController(AgentService agentService, ILogger<ChatController> logger, Kernel kernel, PersistentAgentsClient aiFoundryClient, List<McpClientTool> tools, IHistoryRepository historyRepository)
        {
            _agentService = agentService;
            _logger = logger;
            _kernel = kernel;
            _aiFoundryClient = aiFoundryClient;
            _tools = tools;
            _historyRepository = historyRepository;
        }

        [HttpPost(Name = "chat")]
        [Consumes("application/json")]
        [Produces("application/json", "text/event-stream")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest chatRequest)
        {

            List<ToolCall> toolCallInformation = [];
            IActionResult returnValue;
            StringBuilder responseBuilder = new();

            AzureAIAgent orchestratorAgent = _agentService.OrchestratorAgent;

            // Get or create thread.
            AzureAIAgentThread aiAgentThread = _agentService.GetOrCreateThread(chatRequest.ThreadId);

            if (chatRequest.Stream != true)
            {
                AgentResponseItem<ChatMessageContent> chatResponse = await orchestratorAgent.InvokeAsync(chatRequest.Message, aiAgentThread).FirstAsync();
                
                dynamic response = new ExpandoObject();
                response.message = chatResponse.Message.Content;
                response.threadId = chatResponse.Thread.Id;

                if (chatRequest.Debug == true)
                {
                    response.toolCalls = toolCallInformation;
                }
                returnValue = Ok(response);
            }else
            {
                SetupEventStreamHeaders(aiAgentThread.Id!);
                bool responseStarted = false;
                await Response.WriteAsync("[STARTED]");
                await Response.Body.FlushAsync();

                await foreach (StreamingChatMessageContent chunk in orchestratorAgent.InvokeStreamingAsync(chatRequest.Message, aiAgentThread))
                {
                    string chunkString = chunk.ToString();
                    if(responseStarted == false)
                    {
                        if(chunkString.Trim() != "")
                        {
                            responseStarted = true;
                            responseBuilder.Append(chunk);
                            await Response.WriteAsync(chunkString);
                            await Response.Body.FlushAsync();
                        }
                    }
                    else
                    {
                        await Response.WriteAsync(chunkString);
                        await Response.Body.FlushAsync();
                    }
                }
                await Response.WriteAsync("[DONE]");
                await Response.Body.FlushAsync();
                returnValue = new EmptyResult();
            }

            // Save chat history to repository.
            await _historyRepository.AddMessageToHistoryAsync(chatRequest.UserId, aiAgentThread.Id!, chatRequest.Message, "user");
            await _historyRepository.AddMessageToHistoryAsync(chatRequest.UserId, aiAgentThread.Id!, responseBuilder.ToString(), "assistant");

            return returnValue;
            
        }

        [HttpDelete("/chat/{threadId}")]
        public async Task<IActionResult> DeleteChat([FromRoute] string threadId, [FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(threadId) || string.IsNullOrEmpty(userId))
            {
                return BadRequest("ThreadId and UserId are required.");
            }
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

        private async Task<ChatHistoryAgentThread> BuildAgentThreadAsync (string UserId, string? ThreadId)
        {

            if (!string.IsNullOrEmpty(ThreadId))
            {
                List<ChatMessage> chatHistory = await _historyRepository.GetHistoryAsync(UserId, ThreadId);
                if (chatHistory.Count > 0)
                {
                    ChatHistory history = [];
                    foreach (ChatMessage message in chatHistory)
                    {
                        // Fill thread id if not filled.

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
                    return new ChatHistoryAgentThread(history, ThreadId);
                }
            }
            return new ChatHistoryAgentThread(); 
        }

        private void SetupEventStreamHeaders(string threadId)
        {
            Response.Headers.Append("Content-Type", "text/event-stream");
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");
            Response.Headers.Append("x-thread-id", threadId);
        }
    }
}
