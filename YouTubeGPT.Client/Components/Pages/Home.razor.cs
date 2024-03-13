using Microsoft.AspNetCore.Components;
using Microsoft.SemanticKernel;

namespace YouTubeGPT.Client.Components.Pages;
public partial class Home
{
    [Inject]
    public required Kernel Kernel { get; set; }
}