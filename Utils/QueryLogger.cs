namespace graph_tp.Utils;

public class QueryLogger : IDisposable
{
    private StreamWriter? _writer;
    private string? _currentLogFile;
    private bool _disposed;

    public bool IsActive => _writer != null;

    public void StartLogging(string graphFileName)
    {
        CloseCurrentLog();

        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(graphFileName);
        string dateString = DateTime.Now.ToString("yyyyMMdd");
        string logFileName = $"{fileNameWithoutExt}_{dateString}.log";
        
        string logDirectory = "logs";
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        _currentLogFile = Path.Combine(logDirectory, logFileName);
        _writer = new StreamWriter(_currentLogFile, append: true);
        
        LogSeparator();
        LogMessage($"Sessão iniciada: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        LogMessage($"Arquivo carregado: {graphFileName}");
        LogSeparator();
        _writer.Flush();
    }

    public void LogGraphInfo(int nodeCount, int edgeCount)
    {
        if (_writer == null) return;

        LogMessage($"Informações do Grafo:");
        LogMessage($"  - Vértices: {nodeCount}");
        LogMessage($"  - Arestas: {edgeCount}");
        _writer.Flush();
    }

    public void LogAlgorithmExecution(string algorithmName, string parameters, string result)
    {
        if (_writer == null) return;

        LogSeparator();
        LogMessage($"Algoritmo: {algorithmName}");
        LogMessage($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        
        if (!string.IsNullOrEmpty(parameters))
        {
            LogMessage($"Parâmetros: {parameters}");
        }
        
        LogMessage($"Resultado:");
        foreach (var line in result.Split('\n'))
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                LogMessage($"  {line.TrimEnd()}");
            }
        }
        
        _writer.Flush();
    }

    public void LogError(string error)
    {
        if (_writer == null) return;

        LogMessage($"[ERRO] {error}");
        _writer.Flush();
    }

    private void LogMessage(string message)
    {
        _writer?.WriteLine(message);
    }

    private void LogSeparator()
    {
        _writer?.WriteLine(new string('-', 70));
    }

    public void CloseCurrentLog()
    {
        if (_writer != null)
        {
            LogSeparator();
            LogMessage($"Sessão encerrada: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            LogSeparator();
            _writer.Flush();
            _writer.Close();
            _writer.Dispose();
            _writer = null;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            CloseCurrentLog();
            _disposed = true;
        }
    }
}
