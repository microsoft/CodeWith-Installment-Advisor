using Azure.AI.Agents.Persistent;
using Microsoft.SemanticKernel.Agents.AzureAI;

namespace OrchestratorAPI.Agents
{
    public class FoundryAgentFactory
    {

        private string? _agentId; 
        public async Task<AzureAIAgent> CreateAgentAsync(PersistentAgentsClient client)
        {
            string instructions = """
                You are an agent that provides energy jokes to the user.
                When the user asks for a joke, respond with a humorous energy-related joke.
            """;

            PersistentAgent agentDefinition = await client.Administration.CreateAgentAsync(
                model: "gpt-4o",
                name: "joke-agent",
                instructions: instructions,
                tools: [new CodeInterpreterToolDefinition()]
            );

            // Set agentID for later deletion
            _agentId = agentDefinition.Id;

            return new(agentDefinition, client);
        }

        public async Task<AzureAIAgent> GetAgentAsync(PersistentAgentsClient client, string agentId)
        {

            PersistentAgent agentDefinition = await client.Administration.GetAgentAsync(agentId);

            _agentId = agentDefinition.Id;

            return new(agentDefinition, client);
        }

        public async Task<bool> DeleteAgentAsync(PersistentAgentsClient client)
        {
            if (!string.IsNullOrEmpty(_agentId))
            {
                return await client.Administration.DeleteAgentAsync(_agentId);
            }
            
            return false;
        }
    }
}
