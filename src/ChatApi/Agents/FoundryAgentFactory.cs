using Azure.AI.Agents.Persistent;
using Microsoft.SemanticKernel.Agents.AzureAI;

namespace InstallmentAdvisor.ChatApi.Agents
{
    public class FoundryAgentFactory
    {
        public static async Task<AzureAIAgent> CreateAgentAsync(PersistentAgentsClient client)
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

            return new(agentDefinition, client);
        }

        public static async Task<AzureAIAgent> GetAgentAsync(PersistentAgentsClient client, string agentId)
        {

            PersistentAgent agentDefinition = await client.Administration.GetAgentAsync(agentId);


            return new(agentDefinition, client);
        }

        public static async Task<bool> DeleteAgentAsync(PersistentAgentsClient client, string agentId)
        {
            if (!string.IsNullOrEmpty(agentId))
            {
                return await client.Administration.DeleteAgentAsync(agentId);
            }
            
            return false;
        }
    }
}
