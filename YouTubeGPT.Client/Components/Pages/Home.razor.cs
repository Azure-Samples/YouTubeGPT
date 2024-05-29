using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Memory;
using YouTubeGPT.Client.Models;
using YouTubeGPT.Client.Plugins;
using YouTubeGPT.Ingestion;

namespace YouTubeGPT.Client.Components.Pages;
public partial class Home
{
    [Inject]
    public required Kernel Kernel { get; set; }

    [Inject]
    public required MetadataDbContext MetadataDbContext { get; set; }

    [Inject]
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    public required TextMemoryPlugin MemoryPlugin { get; set; }
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    [Inject]
    public required CollectionSelection CollectionSelectionPlugin { get; set; }

    [CascadingParameter(Name = nameof(IsReversed))]
    public required bool IsReversed { get; set; }

    private string Prompt = "";

    private readonly Dictionary<UserQuestion, string?> _questionAndAnswerMap = [];

    private bool _isReceivingResponse = false;

    protected override void OnInitialized()
    {
        SetupSemanticKernel();
    }

    private void SetupSemanticKernel()
    {
        Kernel.ImportPluginFromObject(MemoryPlugin);

        Kernel.ImportPluginFromObject(CollectionSelectionPlugin, nameof(CollectionSelection));
    }

    private async Task OnAskQuestionAsync()
    {
        if (string.IsNullOrEmpty(Prompt))
        {
            return;
        }

        _isReceivingResponse = true;

        ChatHistory history = [];
        history.AddSystemMessage($"""
            You are an AI Assistant that helps people find YouTube videos that are relevant to something they are wanting to learn about.

            You will need to look for the right collection to access using a provided function before you can perform the memory request.

            Return a summary answer to the persons enquiry, as well as links to the videos that they are interested in.

            Format the response as markdown.
            """);

        UserQuestion userQuestion = new(Prompt, DateTime.Now);
        history.AddUserMessage(userQuestion.Question);
        _questionAndAnswerMap.Add(userQuestion, null);

        var chatCompletions = Kernel.GetRequiredService<IChatCompletionService>();

        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        var response = await chatCompletions.GetChatMessageContentsAsync(
            history,
            openAIPromptExecutionSettings,
            Kernel);

        var r = response.FirstOrDefault();
        if (r is not null && !string.IsNullOrEmpty(r.Content))
        {
            _questionAndAnswerMap[userQuestion] = r.Content;
        }
        else
        {
            _questionAndAnswerMap[userQuestion] = "The AI was unable to generate a response.";
        }

        Prompt = "";
        _isReceivingResponse = false;
    }

    private void OnClearChat()
    {
        Prompt = "";
        _questionAndAnswerMap.Clear();
    }

    record CollectionInfo(string ChannelId, string Title);
}
