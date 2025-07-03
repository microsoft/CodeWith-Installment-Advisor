using InstallmentAdvisor.FoundryAgentProvisioner.Models;
using Newtonsoft.Json;
namespace InstallmentAdvisor.FoundryAgentProvisioner.Utils;
public static class ParseJSONDocument
{
    public static List<IndexDocument> ParseJsonDocuments()
    {
        string[] files = Directory.GetFiles("Documents");
        List<IndexDocument>? documents = new();
        foreach (string file in files)
        {
            using StreamReader reader = new(file);
            var json = reader.ReadToEnd();
            List<IndexDocument> fileDocs = JsonConvert.DeserializeObject<List<IndexDocument>>(json);
            if(fileDocs != null)
            {
                documents.AddRange(fileDocs);
            }
        }

        return documents;
    }
}
