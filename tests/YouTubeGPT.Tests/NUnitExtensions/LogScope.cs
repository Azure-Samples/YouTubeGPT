namespace YouTubeGPT.Tests.NUnitExtensions;

internal class LogScope(object state)
{
    private static readonly AsyncLocal<LogScope?> _value = new();

    public object State { get; } = state;

    internal static LogScope? Current
    {
        get { return _value.Value; }
        set { _value.Value = value; }
    }

    internal LogScope? Parent { get; private set; }

    public override string? ToString() => State.ToString();

    internal static IDisposable Push(object state)
    {
        var temp = Current;

        Current = new LogScope(state)
        {
            Parent = temp,
        };

        return new DisposableScope();
    }

    private sealed class DisposableScope : IDisposable
    {
        public void Dispose() => Current = Current?.Parent;
    }
}