using graph_tp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace graph_tp.Utils;

public static class OutputFormatter
{
    public static void PrintSuccess(string message)
        => Console.WriteLine(message);

    public static void PrintError(string message)
        => Console.WriteLine(message);

    public static void PrintWarning(string message)
        => Console.WriteLine(message);

    public static void PrintInfo(string message)
        => Console.WriteLine(message);

    public static void PrintGraphInfo(Graph graph)
    {
        Console.WriteLine("Informações do Grafo:");
        Console.WriteLine($"  Vértices: {graph.VertexCount}");
        Console.WriteLine($"  Arestas: {graph.EdgesCount}");
    }

    public static void PrintDijkstraResult(object result)
        => Console.WriteLine(result?.ToString() ?? string.Empty);

    public static void PrintMaxFlowResult(object result)
        => Console.WriteLine(result?.ToString() ?? string.Empty);

    public static void PrintMSTResult(object result)
        => Console.WriteLine(result?.ToString() ?? string.Empty);

    public static void PrintColoringResult(object result)
        => Console.WriteLine(result?.ToString() ?? string.Empty);

    public static void PrintEulerianResult(object result)
        => Console.WriteLine(result?.ToString() ?? string.Empty);

    public static void PrintHamiltonianResult(object result)
        => Console.WriteLine(result?.ToString() ?? string.Empty);
}