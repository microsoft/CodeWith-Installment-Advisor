using InstallmentAdvisor.ChatApi.Models;

namespace InstallmentAdvisor.ChatApi.Repositories
{
    public interface IHistoryRepository
    {
        Task<List<ChatMessage>> GetHistoryAsync(string userId, string threadId);
        Task<bool> DeleteHistoryAsync(string userId, string threadId);
        Task<bool> AddMessageToHistoryAsync(string userId, string threadId, string message, string role);
        
    }
}
