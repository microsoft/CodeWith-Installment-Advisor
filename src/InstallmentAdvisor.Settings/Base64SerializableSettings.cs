using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InstallmentAdvisor.Settings;

public abstract class Base64SerializableSettings<T> where T : class
{
    public virtual string ToBase64String()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IncludeFields = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        };
        var json = JsonSerializer.Serialize(this as T, options);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    public static T FromBase64String(string base64)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IncludeFields = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        return JsonSerializer.Deserialize<T>(json, options);
    }
}