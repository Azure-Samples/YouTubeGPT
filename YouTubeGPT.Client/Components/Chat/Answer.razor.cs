// Copyright (c) Microsoft. All rights reserved.

namespace YouTubeGPT.Client.Components.Chat;

public sealed partial class Answer
{
    [Parameter, EditorRequired]
    public string? ProvidedAnswer { get; set; }


    [GeneratedRegex("^(\\s*<br\\s*/?>\\s*)+|(\\s*<br\\s*/?>\\s*)+$", RegexOptions.Multiline)]
    private static partial Regex HtmlLineBreakRegex();
}
