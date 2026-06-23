namespace graph_tp.Utils;

public class QueryLogger : IDisposable
{
    private readonly object _sync = new();
    private StreamWriter? _writer;
    private string? _currentLogFile;
    private bool _disposed;

    public bool IsActive => _writer != null;
    public string? CurrentLogFile => _currentLogFile;

    public void StartSession(string? graphFileName = null)
    {
        lock (_sync)
        {
            CloseCurrentLogInternal();

            string logDirectory = "logs";
            Directory.CreateDirectory(logDirectory);

            string sessionStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            string graphPrefix = string.IsNullOrWhiteSpace(graphFileName)
                ? "session"
                : Path.GetFileNameWithoutExtension(graphFileName);

            _currentLogFile = Path.Combine(logDirectory, $"{graphPrefix}_{sessionStamp}.log");
            _writer = new StreamWriter(_currentLogFile, append: false) { AutoFlush = true };

            WriteLineInternal("=== Sessao iniciada ===");
            WriteLineInternal($"Inicio: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            if (!string.IsNullOrWhiteSpace(graphFileName))
                WriteLineInternal($"Grafo carregado: {graphFileName}");
        }
    }

    public void LogUserAction(string message) => WriteEntry("USUARIO", message);
    public void LogSystemAction(string message) => WriteEntry("SISTEMA", message);
    public void LogAlgorithmAction(string message) => WriteEntry("ALGORITMO", message);
    public void LogError(string message) => WriteEntry("ERRO", message);

    public void LogGraphInfo(int nodeCount, int edgeCount)
    {
        WriteEntry("GRAFO", $"Vertices: {nodeCount}; Arestas: {edgeCount}");
    }

    public void LogAlgorithmExecution(string algorithmName, string parameters, string result)
    {
        lock (_sync)
        {
            if (_writer == null) return;

            WriteLineInternal("--- Execucao de algoritmo ---");
            WriteLineInternal($"Algoritmo: {algorithmName}");
            WriteLineInternal($"Horario: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            if (!string.IsNullOrWhiteSpace(parameters))
                WriteLineInternal($"Parametros: {parameters}");

            if (!string.IsNullOrWhiteSpace(result))
            {
                WriteLineInternal("Resultado:");
                foreach (var line in result.Split('\n'))
                {
                    var trimmed = line.TrimEnd();
                    if (!string.IsNullOrWhiteSpace(trimmed))
                        WriteLineInternal($"  {trimmed}");
                }
            }
        }
    }

    public void CloseCurrentLog()
    {
        lock (_sync)
        {
            CloseCurrentLogInternal();
        }
    }

    private void WriteEntry(string category, string message)
    {
        lock (_sync)
        {
            if (_writer == null) return;
            WriteLineInternal($"[{DateTime.Now:HH:mm:ss}] [{category}] {message}");
        }
    }

    private void WriteLineInternal(string message)
    {
        _writer?.WriteLine(message);
    }

    private void CloseCurrentLogInternal()
    {
        if (_writer == null)
            return;

        WriteLineInternal($"Fim: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        WriteLineInternal("=== Sessao encerrada ===");
        _writer.Dispose();
        _writer = null;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        CloseCurrentLog();
        _disposed = true;
    }
}
