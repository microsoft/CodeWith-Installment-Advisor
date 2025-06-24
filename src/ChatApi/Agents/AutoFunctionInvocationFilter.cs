using Microsoft.SemanticKernel;
using InstallmentAdvisor.ChatApi.Models;

namespace InstallmentAdvisor.ChatApi.Agents;

public class AutoFunctionInvocationFilter(List<ToolCall> toolCallList) : IAutoFunctionInvocationFilter
{
    public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
    {
        await next(context);
        var parametersList = new List<ToolParameter>();

        if (context.Arguments != null)
        {
            parametersList = context.Arguments.Select(p => new ToolParameter { Key = p.Key, Value = p.Value?.ToString() }).ToList();
        }
        var response = context.Result.GetValue<ChatMessageContent[]>()?.First();
        ToolCall toolCallInfo = new ToolCall
        {
            FunctionName = context.Function.Name,
            PluginName = context.Function.PluginName!,
            Parameters = parametersList,
            Response = response?.Content
        };

        toolCallList.Add(toolCallInfo);
    }
}
