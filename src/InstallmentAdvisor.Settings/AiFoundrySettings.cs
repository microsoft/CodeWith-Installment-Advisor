namespace InstallmentAdvisor.Settings;

public class AiFoundrySettings : Base64SerializableSettings<AiFoundrySettings>
{
    public const string Key = "AiFoundry";

    public string ModelName { get; set; }

    public string OpenAiBaseUrl { get; set; }

    public string AiFoundryProjectEndpoint { get; set; }

    public override string ToBase64String()
    {
        return base.ToBase64String();
    }
}
