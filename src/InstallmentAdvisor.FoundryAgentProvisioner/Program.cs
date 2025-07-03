using Azure;
using Azure.AI.Agents.Persistent;
using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using InstallmentAdvisor.ChatApi;
using InstallmentAdvisor.ChatApi.Helpers;
using InstallmentAdvisor.FoundryAgentProvisioner.Models;
using InstallmentAdvisor.FoundryAgentProvisioner.Utils;
using InstallmentAdvisor.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.Agents.AzureAI;

Console.WriteLine("Provisioner starting...");

// Build configuration provider
IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>(optional: true)
    .Build();

// Use configuration to initialize other objects
string environmentName = configuration["EnvironmentName"] ?? "Production";
DefaultAzureCredentialOptions azureCredentialOptions = CredentialHelper.GetDefaultAzureCredentialOptions(environmentName);
var azureCredential = new DefaultAzureCredential(azureCredentialOptions);

Console.WriteLine("Agent Provisioning starting...");

AiFoundrySettings aiFoundrySettings = AiFoundrySettings.FromBase64String(configuration[AiFoundrySettings.Key]!);

PersistentAgentsClient aiFoundryClient = AzureAIAgent.CreateAgentsClient(aiFoundrySettings.AiFoundryProjectEndpoint, azureCredential);

AgentsSettings agentsSettings = AgentsSettings.FromBase64String(configuration[AgentsSettings.Key]!);

// Fetch current agents from Foundry
var existingAgents = aiFoundryClient.Administration.GetAgents();
var existingAgentsDict = existingAgents
    .GroupBy(a => a.Name)
    .ToDictionary(g => g.Key, g => g.First());

// Iterate through AgentConstants
var requiredAgentNames = new List<string>
{
    AgentConstants.VISUALIZATION_AGENT_NAME,
    AgentConstants.INSTALLMENT_RULE_EVALUATION_AGENT_NAME,
    AgentConstants.SCENARIO_AGENT_NAME,
    AgentConstants.ORCHESTRATOR_AGENT_NAME,
    AgentConstants.UPDATE_INSTALLMENT_AMOUNT_AGENT_NAME
};
foreach (var agentConfiguration in agentsSettings.Agents)
{
    string? configuredPrompt = agentConfiguration.Prompt;

    bool hasCodeInterpreterTool = agentConfiguration.HasCodeInterpreterTool;

    if (string.IsNullOrWhiteSpace(agentConfiguration.Prompt))
    {
        Console.WriteLine($"No prompt configured for agent '{agentConfiguration.AgentName}', skipping.");
        continue;
    }

    var tools = new List<ToolDefinition>();
    if (hasCodeInterpreterTool)
    {
        // Add code interpreter tool if configured
        tools.Add(new CodeInterpreterToolDefinition());
    }

    if (existingAgentsDict.TryGetValue(agentConfiguration.AgentName, out var existingAgent))
    {
        // Agent exists, check if prompt matches
        if (ShouldUpdateAgent(existingAgent, agentConfiguration, tools))
        {
            // Update agent with new prompt
            await aiFoundryClient.Administration.UpdateAgentAsync(existingAgent.Id, 
                instructions: agentConfiguration.Prompt, 
                model: agentConfiguration.ModelName, 
                tools: tools, 
                temperature: agentConfiguration.Temperature, 
                topP: agentConfiguration.TopP
            );
            Console.WriteLine($"Updated agent '{agentConfiguration.AgentName}' with new prompt.");
        }
        else
        {
            Console.WriteLine($"Agent '{agentConfiguration.AgentName}' is up to date, utilize {existingAgent.Id}.");
        }
    }
    else
    {
        // Agent does not exist, create it
        var createdAgent = await aiFoundryClient.Administration.CreateAgentAsync(
            name: agentConfiguration.AgentName,
            model: agentConfiguration.ModelName,
            instructions: configuredPrompt,
            tools: tools,
            temperature: agentConfiguration.Temperature,
            topP: agentConfiguration.TopP
        );
        Console.WriteLine($"Created agent '{agentConfiguration.AgentName}' with {createdAgent.Value.Id}.");
    }
}

Console.WriteLine("Agent Provisioner completed!");

Console.WriteLine("Index Provisioning starting...");

AzureAiSearchSettings aiSearchSettings = AzureAiSearchSettings.FromBase64String(configuration[AzureAiSearchSettings.Key]!);
string indexName = aiSearchSettings.IndexName;
string endpoint = aiSearchSettings.Endpoint;
string embeddingModel = aiSearchSettings.EmbeddingDeploymentName;

AzureKeyCredential credential = new AzureKeyCredential(aiSearchSettings.ApiKey);
SearchIndexClient indexClient = new SearchIndexClient(new Uri(endpoint), credential);

bool indexExists = true;
try
{
    var index = await indexClient.GetIndexAsync(indexName);
}
catch(Exception e)
{
    indexExists = false;
}

if (indexExists == false)
{
    Console.WriteLine($"Index '{indexName}' does not exist, creating it...");

    var vectorSearch = new VectorSearch
    {
        Algorithms =
        {
            new HnswAlgorithmConfiguration("hnsw-configuration")
            {
                Parameters = new HnswParameters
                {
                    Metric = VectorSearchAlgorithmMetric.Cosine,
                    M = 10,
                    EfConstruction = 400,
                    EfSearch = 400
                }
            }
        },
        Profiles =
        {
            new VectorSearchProfile("vector-profile-embedding", "hnsw-configuration")
        }
    };

    var newIndex = new SearchIndex(indexName)
    {
        Fields =
    {
        new SimpleField("id", SearchFieldDataType.String) { IsKey = true, IsFilterable = true, IsSortable = false, IsFacetable = false },
        new SearchableField("content") { IsFilterable = true, IsSortable = true },
        new SearchableField("title") { IsFilterable = true, IsSortable = true },
        new SearchableField("summary") { IsFilterable = true, IsSortable = true },
        new SearchableField("article") { IsFilterable = true, IsSortable = true, IsFacetable = true},
        new SearchField("keywords", SearchFieldDataType.Collection(SearchFieldDataType.String)) { IsSearchable = true, IsFilterable = true, IsSortable = false },
        new SearchField("content_vector", SearchFieldDataType.Collection(SearchFieldDataType.Single))
        {
            IsSearchable = true,
            IsFilterable = false,
            IsSortable = false,
            IsFacetable = false,
            IsKey = false,
            IsStored = true,
            VectorSearchDimensions = 1536,
            VectorSearchProfileName = "vector-profile-embedding"
        },
       
    },
        Similarity = new BM25Similarity(),
        VectorSearch = vectorSearch
    };

    await indexClient.CreateIndexAsync(newIndex);

    SearchClient searchClient = new SearchClient(
        new Uri(endpoint),
        indexName,
        credential
    );

    List<IndexDocument> docs = ParseJSONDocument.ParseJsonDocuments();

    AzureOpenAIClient azureClient = new(new Uri(aiFoundrySettings.OpenAiBaseUrl),azureCredential);

    var embeddingClient = azureClient.GetEmbeddingClient(embeddingModel);

    // For each doc, generate an embedding.
    foreach (var doc in docs)
    {
        if (doc.content_vector == null || doc.content_vector.Count == 0)
        {
            var embedding = await embeddingClient.GenerateEmbeddingAsync(doc.Content);
            var embeddingArray = embedding.Value.ToFloats().ToArray();
            doc.content_vector = embeddingArray.ToList();

        }
    }
    await searchClient.UploadDocumentsAsync(docs);
}
else
{
    Console.WriteLine($"Index '{indexName}' already exists, skipping creation.");
}

Console.WriteLine("Index Provisioning completed!");


static bool ShouldUpdateAgent(dynamic existingAgent, AgentConfigurationSettings agentConfiguration, List<ToolDefinition> tools)
{
    if (existingAgent.Instructions != agentConfiguration.Prompt)
        return true;
    if (existingAgent.Model != agentConfiguration.ModelName)
        return true;
    if (existingAgent.Temperature != agentConfiguration.Temperature)
        return true;
    if (existingAgent.TopP != agentConfiguration.TopP)
        return true;

    if (existingAgent.Tools is IEnumerable<ToolDefinition> existingTools)
    {
        if (existingTools.Count() != tools.Count)
            return true;
        var existingTypes = existingTools.Select(t => t.GetType()).OrderBy(t => t.FullName).ToList();
        var newTypes = tools.Select(t => t.GetType()).OrderBy(t => t.FullName).ToList();
        if (!existingTypes.SequenceEqual(newTypes))
            return true;
    }

    return false;
}
