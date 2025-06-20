using Newtonsoft.Json;

namespace Domain
{
    public record ToolCall
    {
        public required string FunctionName { get; set; }
        public required string PluginName { get; set; }

        public List<ToolParameter>? Parameters { get; set; }

        public string? Response { get; set; }

    }
    
    public record ToolParameter
    {
        public required string Key { get; set; }
        public string? Value { get; set; }
    }
}
