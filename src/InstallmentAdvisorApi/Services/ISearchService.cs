using InstallmentAdvisor.DataApi.Models;

namespace InstallmentAdvisor.DataApi.Services;

public interface ISearchService
{
    public Task<string> SearchAsync(string query);

    public Task<List<SearchDocumentModel>> SearchVectorAsync(float[] queryVector);

}
