using Microsoft.Extensions.Logging;

namespace AgentDashboard.TicketTracking.Infrastructure.IntegrationTests.GitHub.Fakes;

public sealed record LogEntry(
    LogLevel Level,
    string Message,
    IReadOnlyDictionary<string, object?> State);

public sealed class RecordingLogger<T> : ILogger<T>
{
    private readonly List<LogEntry> _entries = [];
    private readonly Lock _gate = new();

    public IReadOnlyList<LogEntry> Entries
    {
        get
        {
            lock (_gate)
            {
                return [.. _entries];
            }
        }
    }

    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        var dictionary = new Dictionary<string, object?>(StringComparer.Ordinal);
        if (state is IReadOnlyList<KeyValuePair<string, object?>> pairs)
        {
            foreach (var pair in pairs)
            {
                dictionary[pair.Key] = pair.Value;
            }
        }

        lock (_gate)
        {
            _entries.Add(new LogEntry(logLevel, formatter(state, exception), dictionary));
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}
