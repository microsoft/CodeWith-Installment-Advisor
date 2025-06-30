namespace InstallmentAdvisor.Settings;

public class AgentConfigurationSettings
{
    public string AgentName { get; set; }
    public string Prompt { get; set; }

    public string ModelName { get; set; }

    public bool HasCodeInterpeterTool { get; set; }

    public List<string> AvailableMcpTools { get; set; }
}