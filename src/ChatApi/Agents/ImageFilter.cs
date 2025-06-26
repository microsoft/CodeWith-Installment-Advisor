using Azure.AI.Agents.Persistent;
using Microsoft.SemanticKernel;

namespace InstallmentAdvisor.ChatApi.Agents;
public class ImageFilter(PersistentAgentsClient aiFoundryClient, List<string> images) : IFunctionInvocationFilter
{
    private readonly PersistentAgentsClient _aiFoundryClient = aiFoundryClient;
    private readonly List<string> _images = images;
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        // Perform the function call.
        await next(context);

        // Check if the result has an image.
        ChatMessageContent[]? messages = context.Result.GetValue<ChatMessageContent[]>();

        if (messages != null)
        {
            var imageMessages = messages.Where(m => m.Items.Any(i => i.GetType() == typeof(FileReferenceContent)));

            // For each nested item in imageMessages, get the fileid.
            foreach (var message in imageMessages)
            {
                foreach (var item in message.Items)
                {
                    if (item is FileReferenceContent fileReferenceContent)
                    {
                        // Process the file reference content as needed.
                        var contents = await _aiFoundryClient.Files.GetFileContentAsync(fileReferenceContent.FileId);
                        var bytearray = contents.Value.ToArray();
                        // Add to list.
                        var base64Contents = Convert.ToBase64String(bytearray);
                        _images.Add(base64Contents);
                    }
                }
            }
        }
    }
}
