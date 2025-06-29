namespace InstallmentAdvisor.Settings;

public class AgentsSettings : Base64SerializableSettings<AgentsSettings>
{
    public const string Key = "AgentSettings";

    public List<AgentConfigurationSettings> Agents { get; set; } = [];
}
