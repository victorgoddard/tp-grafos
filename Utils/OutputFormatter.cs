//using graph_tp.Models;
//using graph_tp.Algorithms.Results;

//namespace graph_tp.Utils;

//public static class OutputFormatter
//{
//    public static void PrintHeader(string title)
//    {
//        Console.WriteLine();
//        Console.ForegroundColor = ConsoleColor.Cyan;
//        Console.WriteLine("═══════════════════════════════════════════════════════════");
//        Console.WriteLine($"  {title}");
//        Console.WriteLine("═══════════════════════════════════════════════════════════");
//        Console.ResetColor();
//        Console.WriteLine();
//    }

//    public static void PrintSuccess(string message)
//    {
//        Console.ForegroundColor = ConsoleColor.Green;
//        Console.WriteLine($"✓ {message}");
//        Console.ResetColor();
//    }

//    public static void PrintError(string message)
//    {
//        Console.ForegroundColor = ConsoleColor.Red;
//        Console.WriteLine($"✗ {message}");
//        Console.ResetColor();
//    }

//    public static void PrintWarning(string message)
//    {
//        Console.ForegroundColor = ConsoleColor.Yellow;
//        Console.WriteLine($"⚠ {message}");
//        Console.ResetColor();
//    }

//    public static void PrintInfo(string message)
//    {
//        Console.ForegroundColor = ConsoleColor.White;
//        Console.WriteLine($"ℹ {message}");
//        Console.ResetColor();
//    }

//    public static void PrintDijkstraResult(DijkstraResult result)
//    {
//        PrintHeader("RESULTADO: Rota de Menor Custo (Dijkstra)");

//        if (!result.Success)
//        {
//            PrintError("Não foi possível encontrar um caminho entre origem e destino.");
//            return;
//        }

//        PrintSuccess($"Custo Total: R$ {result.TotalCost:F2}");
//        Console.WriteLine();
//        Console.WriteLine("Caminho:");
        
//        for (int i = 0; i < result.Path.Count; i++)
//        {
//            var edge = result.Path[i];
//            Console.WriteLine($"  {i + 1}. Hub {edge.Source.Id} → Hub {edge.Target.Id} (Custo: R$ {edge.Cost:F2})");
//        }
//    }

//    public static void PrintMaxFlowResult(MaxFlowResult result)
//    {
//        PrintHeader("RESULTADO: Capacidade Máxima de Escoamento (Edmonds-Karp)");

//        PrintSuccess($"Fluxo Máximo: {result.MaxFlow:F2} toneladas");
//        Console.WriteLine();

//        if (result.BottleneckEdges.Count > 0)
//        {
//            Console.WriteLine("Arestas Críticas (Gargalos):");
//            foreach (var edge in result.BottleneckEdges)
//            {
//                Console.WriteLine($"  • Hub {edge.Source.Id} → Hub {edge.Target.Id} (Capacidade: {edge.Capacity:F2}t)");
//            }
//        }
//    }

//    public static void PrintMSTResult(MSTResult result)
//    {
//        PrintHeader("RESULTADO: Expansão da Rede - Árvore Geradora Mínima (Kruskal)");

//        PrintSuccess($"Custo Total de Instalação: R$ {result.TotalCost:F2}");
//        Console.WriteLine();
//        Console.WriteLine($"Conexões Necessárias ({result.Edges.Count} rotas):");
        
//        foreach (var edge in result.Edges.OrderBy(e => e.Cost))
//        {
//            Console.WriteLine($"  • Hub {edge.Source.Id} ↔ Hub {edge.Target.Id} (Custo: R$ {edge.Cost:F2})");
//        }
//    }

//    public static void PrintColoringResult(ColoringResult result)
//    {
//        PrintHeader("RESULTADO: Agendamento de Manutenções (Welsh-Powell)");

//        PrintSuccess($"Número Mínimo de Turnos: {result.Shifts.Count}");
//        Console.WriteLine();

//        foreach (var shift in result.Shifts.OrderBy(s => s.Key))
//        {
//            Console.ForegroundColor = ConsoleColor.Yellow;
//            Console.WriteLine($"Turno {shift.Key}:");
//            Console.ResetColor();
            
//            foreach (var edge in shift.Value)
//            {
//                Console.WriteLine($"  • Rota: Hub {edge.Source.Id} → Hub {edge.Target.Id}");
//            }
//            Console.WriteLine();
//        }
//    }

//    public static void PrintEulerianResult(EulerianResult result)
//    {
//        PrintHeader("RESULTADO: Rota de Inspeção - Caminho/Ciclo Euleriano");

//        if (!result.IsPossible)
//        {
//            PrintError("Não é possível percorrer todas as rotas sem repetições.");
//            PrintInfo("Condições não satisfeitas para um caminho ou ciclo euleriano.");
//            return;
//        }

//        PrintSuccess("É possível percorrer todas as rotas sem repetições!");
//        Console.WriteLine();
//        Console.WriteLine("Sequência de Rotas:");
        
//        for (int i = 0; i < result.Path.Count; i++)
//        {
//            var edge = result.Path[i];
//            Console.WriteLine($"  {i + 1}. Hub {edge.Source.Id} → Hub {edge.Target.Id}");
//        }
//    }

//    public static void PrintHamiltonianResult(HamiltonianResult result)
//    {
//        PrintHeader("RESULTADO: Rota de Inspeção - Ciclo Hamiltoniano");

//        if (!result.IsPossible)
//        {
//            PrintError("Não foi encontrado um ciclo hamiltoniano.");
//            PrintInfo("Não é possível visitar todos os hubs exatamente uma vez e retornar à origem.");
//            return;
//        }

//        PrintSuccess("Ciclo hamiltoniano encontrado!");
//        Console.WriteLine();
//        Console.WriteLine("Sequência de Hubs:");
        
//        for (int i = 0; i < result.Cycle.Count; i++)
//        {
//            var node = result.Cycle[i];
//            Console.Write($"Hub {node.Id}");
//            if (i < result.Cycle.Count - 1)
//                Console.Write(" → ");
//        }
//        Console.WriteLine();
//    }

//    public static void PrintGraphInfo(Graph graph)
//    {
//        Console.WriteLine();
//        Console.ForegroundColor = ConsoleColor.Cyan;
//        Console.WriteLine("Informações do Grafo:");
//        Console.ResetColor();
//        Console.WriteLine($"  • Número de Hubs: {graph.VertexCount}");
//        Console.WriteLine($"  • Número de Rotas: {graph.VertexCount}");
//        Console.WriteLine();
//    }
//}
