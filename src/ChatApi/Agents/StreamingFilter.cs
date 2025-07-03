using Microsoft.SemanticKernel;
using ModelContextProtocol.Protocol;

namespace InstallmentAdvisor.ChatApi.Agents;
public class StreamingFilter(HttpResponse response) : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        string toolInfo = $"[TOOLCALL] {context.Function.Name} [ENDTOOLCALL]";
        await response.WriteAsync(toolInfo);
        await response.Body.FlushAsync();
        await next(context);
    }
}
