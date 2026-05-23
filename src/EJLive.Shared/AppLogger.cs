namespace EJLive.Shared;

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Critical
}

public sealed class LogEntryEventArgs : EventArgs
{
    public LogEntryEventArgs(DateTimeOffset timestamp, LogLevel level, string source, string message)
    {
        Timestamp = timestamp;
        Level = level;
        Source = source;
        Message = message;
    }

    public DateTimeOffset Timestamp { get; }
    public LogLevel Level { get; }
    public string Source { get; }
    public string Message { get; }
    public string FormattedForUI => $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level}] [{Source}] {Message}";
}

public sealed class AppLogger : IDisposable
{
    private readonly object _sync = new();
    private string _logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Logs");
    private string _component = "app";
    private bool _initialized;

    public static AppLogger Instance { get; } = new();

    public event EventHandler<LogEntryEventArgs>? OnLog;

    private AppLogger()
    {
    }

    public void Initialize(string logDirectory, string component)
    {
        lock (_sync)
        {
            _logDirectory = string.IsNullOrWhiteSpace(logDirectory) ? _logDirectory : logDirectory;
            _component = string.IsNullOrWhiteSpace(component) ? "app" : component;
            Directory.CreateDirectory(_logDirectory);
            _initialized = true;
        }
    }

    public void Debug(string message, string source = "General") => Write(LogLevel.Debug, source, message);
    public void Info(string message, string source = "General") => Write(LogLevel.Info, source, message);
    public void Warning(string message, string source = "General") => Write(LogLevel.Warning, source, message);
    public void Error(string message, string source = "General") => Write(LogLevel.Error, source, message);
    public void Critical(string message, string source = "General") => Write(LogLevel.Critical, source, message);

    public void Write(LogLevel level, string source, string message)
    {
        var entry = new LogEntryEventArgs(DateTimeOffset.Now, level, source, message);
        OnLog?.Invoke(this, entry);

        try
        {
            lock (_sync)
            {
                if (!_initialized)
                    Initialize(_logDirectory, _component);

                var file = Path.Combine(_logDirectory, $"{_component}-{DateTime.Today:yyyyMMdd}.log");
                File.AppendAllText(file, entry.FormattedForUI + Environment.NewLine);
            }
        }
        catch
        {
            // Logging must not interrupt ATM operations.
        }
    }

    public void Dispose()
    {
    }
}
