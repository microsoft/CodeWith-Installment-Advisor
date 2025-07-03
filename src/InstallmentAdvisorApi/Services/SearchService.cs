using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using InstallmentAdvisor.DataApi.Models;
using InstallmentAdvisor.Settings;
using OpenAI.Embeddings;

namespace InstallmentAdvisor.DataApi.Services;

public class SearchService : ISearchService
{
    private readonly AzureAiSearchSettings azureAiSearchSettings;
    private readonly SearchIndexClient azureSearchService;
    private readonly EmbeddingClient openAIClient;

    public SearchService(SearchIndexClient azureSearchService, EmbeddingClient openAIClient, AzureAiSearchSettings azureAiSearchSettings)
    {
        this.azureSearchService = azureSearchService;
        this.openAIClient = openAIClient;
        this.azureAiSearchSettings = azureAiSearchSettings;
    }

    public async Task<string> SearchAsync(string query)
    {
        var embeddings = openAIClient.GenerateEmbedding(query);

        var results = await SearchVectorAsync(embeddings.Value.ToFloats().ToArray());

        List<string> contents = new List<string>();

        foreach (var doc in results)
        {
            contents.Add(doc.Content);
        }

        return string.Join(Environment.NewLine, contents);
    }

    public async Task<List<SearchDocumentModel>> SearchVectorAsync(float[] queryVector)
    {
        var searchClient = azureSearchService.GetSearchClient(azureAiSearchSettings.IndexName);

        var options = new Azure.Search.Documents.SearchOptions
        {
            Size = 5,
            Select = { "id", "content", "title", "summary", "article" },
        };

        options.VectorSearch = new VectorSearchOptions
        {
            Queries =
            {
                new VectorizedQuery(queryVector)
                {
                    KNearestNeighborsCount = 5,
                    Fields = { "content_vector" }
                }
            }
        };

        var response = await searchClient.SearchAsync<SearchDocument>("*", options);

        var output = new List<SearchDocumentModel>();
        await foreach (SearchResult<SearchDocument> result in response.Value.GetResultsAsync())
        {
            output.Add(new SearchDocumentModel
            {
                Id = result.Document["id"] as string,
                Content = result.Document["content"] as string,
                Title = result.Document["title"] as string,
                Summary = result.Document["summary"] as string,
                Article = result.Document["article"] as string
            });
        }

        return output;
    }
}
