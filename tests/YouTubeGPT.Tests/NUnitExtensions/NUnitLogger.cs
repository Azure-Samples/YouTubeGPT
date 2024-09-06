using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;

namespace YouTubeGPT.Tests.NUnitExtensions;

internal class NUnitLogger(string name, TextWriter writer, TimeProvider time) : ILogger
{
    private const string LogLevelPadding = ": ";
    private static readonly string MessagePadding = new(' ', GetLogLevelString(LogLevel.Debug).Length + LogLevelPadding.Length);
    private static readonly string NewLineWithMessagePadding = Environment.NewLine + MessagePadding;

    [ThreadStatic]
    private static StringBuilder? _logBuilder;

    public bool IncludeScopes { get; set; }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return LogScope.Push(state);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        string? message = formatter(state, exception);

        if (!string.IsNullOrEmpty(message) || exception != null)
        {
            WriteMessage(logLevel, eventId.Id, message, exception);
        }
    }

    /// <summary>
    /// Writes a message to the <see cref="ITestOutputHelper"/> or <see cref="IMessageSink"/> associated with the instance.
    /// </summary>
    /// <param name="logLevel">The message to write will be written on this level.</param>
    /// <param name="eventId">The Id of the event.</param>
    /// <param name="message">The message to write.</param>
    /// <param name="exception">The exception related to this message.</param>
    public virtual void WriteMessage(LogLevel logLevel, int eventId, string? message, Exception? exception)
    {
        StringBuilder? logBuilder = _logBuilder;
        _logBuilder = null;

        logBuilder ??= new StringBuilder();

        string logLevelString = GetLogLevelString(logLevel);

        logBuilder.Append(LogLevelPadding);
        logBuilder.Append(name);
        logBuilder.Append('[');
        logBuilder.Append(eventId);
        logBuilder.Append(']');
        logBuilder.AppendLine();

        if (IncludeScopes)
        {
            GetScopeInformation(logBuilder);
        }

        bool hasMessage = !string.IsNullOrEmpty(message);

        if (hasMessage)
        {
            logBuilder.Append(MessagePadding);

            int length = logBuilder.Length;
            logBuilder.Append(message);
            logBuilder.Replace(Environment.NewLine, NewLineWithMessagePadding, length, message!.Length);
        }

        if (exception != null)
        {
            if (hasMessage)
            {
                logBuilder.AppendLine();
            }

            logBuilder.Append(exception);
        }

        // Prefix the formatted message so it renders like this:
        // [{timestamp}] {logLevelString}{message}
        logBuilder.Insert(0, logLevelString);
        logBuilder.Insert(0, "] ");
        logBuilder.Insert(0, time.GetLocalNow().ToString("t", CultureInfo.CurrentCulture));
        logBuilder.Insert(0, '[');

        string line = logBuilder.ToString();

        writer.WriteLine(line);

        logBuilder.Clear();

        if (logBuilder.Capacity > 1024)
        {
            logBuilder.Capacity = 1024;
        }

        _logBuilder = logBuilder;
    }

    private static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Critical => "crit",
            LogLevel.Debug => "dbug",
            LogLevel.Error => "fail",
            LogLevel.Information => "info",
            LogLevel.Trace => "trce",
            LogLevel.Warning => "warn",
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel)),
        };
    }

    private static void GetScopeInformation(StringBuilder builder)
    {
        var current = LogScope.Current;

        var stack = new Stack<LogScope>();
        while (current != null)
        {
            stack.Push(current);
            current = current.Parent;
        }

        var depth = 0;
        static string DepthPadding(int depth) => new(' ', depth * 2);

        while (stack.Count > 0)
        {
            var elem = stack.Pop();
            foreach (var property in StringifyScope(elem))
            {
                builder.Append(MessagePadding)
                       .Append(DepthPadding(depth))
                       .Append("=> ")
                       .Append(property)
                       .AppendLine();
            }

            depth++;
        }
    }

    private static IEnumerable<string?> StringifyScope(LogScope scope)
    {
        if (scope.State is IEnumerable<KeyValuePair<string, object>> pairs)
        {
            foreach (var pair in pairs)
            {
                yield return $"{pair.Key}: {pair.Value}";
            }
        }
        else if (scope.State is IEnumerable<string> entries)
        {
            foreach (var entry in entries)
            {
                yield return entry;
            }
        }
        else
        {
            yield return scope.ToString();
        }
    }
}
