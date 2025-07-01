using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.Agents.Extensions;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Diagnostics.CodeAnalysis;

namespace InstallmentAdvisor.ChatApi.Agents;
public static class AgentAsToolFactory
{
    public static KernelFunction CreateFromAgent(
        Agent agent,
        AzureAIAgentThread aiAgentThread,
        string? functionName = null,
        string? description = null,
        IEnumerable<KernelParameterMetadata>? parameters = null,
        ILoggerFactory? loggerFactory = null)
    {

        async Task<FunctionResult> InvokeAgentAsync(Kernel kernel, KernelFunction function, KernelArguments arguments, CancellationToken cancellationToken)
        {
            arguments.TryGetValue("query", out var query);
            var queryString = query?.ToString() ?? string.Empty;

            AgentInvokeOptions? options = null;

            if (arguments.TryGetValue("instructions", out var instructions) && instructions is not null)
            {
                options = new()
                {
                    AdditionalInstructions = instructions?.ToString() ?? string.Empty
                };
            }

            var response = agent.InvokeAsync(new ChatMessageContent(AuthorRole.User, queryString), aiAgentThread, options, cancellationToken);
            var responseItems = await response.ToArrayAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var chatMessages = responseItems.Select(i => i.Message).ToArray();
            return new FunctionResult(function, chatMessages, kernel.Culture);
        }

        KernelFunctionFromMethodOptions options = new()
        {
            FunctionName = functionName ?? agent.Name,
            Description = description ?? agent.Description,
            Parameters = parameters ?? GetDefaultKernelParameterMetadata(),
            ReturnParameter = new() { ParameterType = typeof(FunctionResult) },
        };

        return KernelFunctionFactory.CreateFromMethod(
                InvokeAgentAsync,
                options);
    }
    private static IEnumerable<KernelParameterMetadata> GetDefaultKernelParameterMetadata()
    {
        return s_kernelParameterMetadata ??= [
            new KernelParameterMetadata("query") { Description = "Available information that will guide in performing this operation.", ParameterType = typeof(string), IsRequired = true },
            new KernelParameterMetadata("instructions") { Description = "Additional instructions for the agent.", ParameterType = typeof(string), IsRequired = true },
        ];
    }

    private static IEnumerable<KernelParameterMetadata>? s_kernelParameterMetadata;
}
