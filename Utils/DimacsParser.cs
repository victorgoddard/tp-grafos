using graph_tp.Models;

namespace graph_tp.Utils;

public static class DimacsParser
{
    public static Graph LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        var graph = new Graph();
        var lines = File.ReadAllLines(filePath);

        if (lines.Length == 0)
        {
            throw new FormatException("Empty file");
        }

        var firstLine = lines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (firstLine.Length != 2)
        {
            throw new FormatException($"Invalid first line. Expected 'V E', got: {lines[0]}");
        }

        if (!int.TryParse(firstLine[0], out int vertexCount) || 
            !int.TryParse(firstLine[1], out int edgeCount))
        {
            throw new FormatException($"Invalid vertex or edge count: {lines[0]}");
        }

        for (int i = 1; i <= vertexCount; i++)
        {
            graph.AddVertex(new Vertex(i));
        }

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length < 3)
            {
                throw new FormatException($"Invalid edge format on line {i + 1}. Expected 'source target cost [capacity]', got: {line}");
            }

            if (!int.TryParse(parts[0], out int source) ||
                !int.TryParse(parts[1], out int target) ||
                !double.TryParse(parts[2], out double cost))
            {
                throw new FormatException($"Invalid edge values on line {i + 1}: {line}");
            }

            double capacity = double.PositiveInfinity;
            if (parts.Length >= 4)
            {
                if (!double.TryParse(parts[3], out capacity))
                {
                    throw new FormatException($"Invalid capacity on line {i + 1}: {parts[3]}");
                }
            }

            var sourceNode = graph.GetVertex(source);
            var targetNode = graph.GetVertex(target);

            if (sourceNode == null || targetNode == null)
            {
                throw new FormatException($"Invalid node reference on line {i + 1}. Source: {source}, Target: {target}");
            }

            graph.AddEdge(new Edge(sourceNode, targetNode, cost, capacity));
        }

        return graph;
    }

    public static void SaveToFile(Graph graph, string filePath)
    {
        using var writer = new StreamWriter(filePath);
        
        writer.WriteLine($"{graph} {graph.EdgesCount}");

        foreach (var edge in graph.GetAllEdges())
        {
            var sourceId = edge.Source.GetHashCode(); var targetId = edge.Target.GetHashCode(); 
            var loadValue = edge.LoadValue; var capacity = edge.Capacity;
                
            if (edge.Capacity == double.PositiveInfinity)
            {
                writer.WriteLine($"{sourceId} {targetId} {loadValue}");
            }
            else
            {
                writer.WriteLine($"{sourceId} {targetId} {loadValue} {capacity}");
            }
        }
    }
}
