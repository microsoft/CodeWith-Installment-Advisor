using Domain;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace Infrastructure
{
    public class CosmosHistoryRepository : IHistoryRepository
    {

        private Container _container;

        public CosmosHistoryRepository(Container cosmosDbContainer)
        {
            _container = cosmosDbContainer;
        }

        public async Task<List<ChatMessage>> GetHistoryAsync(string userId, string threadId)
        {
            var messagesQuery = _container
                .GetItemLinqQueryable<ChatMessage>(allowSynchronousQueryExecution: true)
                .Where(m => m.ThreadId == threadId && m.UserId == userId)
                .OrderBy(m => m.Created);

            var iterator = messagesQuery.ToFeedIterator();

            var messages = new List<ChatMessage>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                messages.AddRange(response);
            }

            return messages;
        }

        public async Task<bool> DeleteHistoryAsync(string userId, string threadId)
        {
            var messages = await this.GetHistoryAsync(userId, threadId);
            if (messages.Count == 0)
            {
                return false; // No messages to delete
            }
            
            var batch = _container.CreateTransactionalBatch(new PartitionKey(userId));
            
            foreach (ChatMessage message in messages)
            {
                batch.DeleteItem(message.Id);
            }
            
            var batchResponse = await batch.ExecuteAsync();
            if (!batchResponse.IsSuccessStatusCode)
            {
                throw new Exception("Failed to delete messages in bulk.");
            }
            return true;
        }

        public async Task<bool> AddMessageToHistoryAsync(string userId, string threadId, string message, string role)
        {
            string messageId = Guid.NewGuid().ToString();
            DateTime now = DateTime.Now;

            ChatMessage newMessage = new()
            {
                Id = messageId,
                ThreadId = threadId,
                UserId = userId,
                Role = role,
                Content = message,
                Created = now
            };

            var response = await _container.CreateItemAsync<ChatMessage>(newMessage, new PartitionKey(userId));
            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                throw new Exception("Failed to create a new message in history.");
            }
            return true;
        }
    }
}
