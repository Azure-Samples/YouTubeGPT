namespace YouTubeGPT.Client.Models;

public readonly record struct UserQuestion(
    string Question,
    DateTime AskedOn);