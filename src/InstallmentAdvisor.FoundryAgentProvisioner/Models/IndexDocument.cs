namespace InstallmentAdvisor.FoundryAgentProvisioner.Models;
public class IndexDocument
{
    public string Id { get; set; }
    public string Content { get; set; }
    public string Title { get; set; }
    public string Summary { get; set; }
    public string Article { get; set; }
    public List<string>? Keywords { get; set; }

    public List<float>? content_vector { get; set; }
}