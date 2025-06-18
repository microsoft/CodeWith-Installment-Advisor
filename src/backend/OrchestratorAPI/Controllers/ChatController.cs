using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
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
        [Produces("application/json")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest chatRequest)
        {

            var agent = new OrchestratorAgent(_kernel);
            object response = await agent.ChatAsync(chatRequest.Message);

            // Return response string as json ok response.
            return Ok(response);

        }
    }
}
