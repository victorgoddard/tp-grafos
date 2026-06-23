using graph_tp.Models;
using graph_tp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace graph_tp.Algorithms
{
    public class BellmanFordAlgorithm
    {
        public class BellmanFordResult
        {
            public int Source { get; set; }
            public Dictionary<int, double> Distances { get; } = new Dictionary<int, double>();
            public Dictionary<int, int?> Predecessors { get; } = new Dictionary<int, int?>();
        }

        public static BellmanFordResult RunBellmanFord(Graph graph, int source, QueryLogger? logger = null)
        {
            if (!graph.Containsvertex(source))
                throw new ArgumentException($"Vértice origem {source} não existe no grafo.");

            logger?.LogAlgorithmAction($"Bellman-Ford iniciado com origem {source}.");

            var vertices = graph.GetAllvertexs()
                .Select(vertex => vertex.GetHashCode())
                .ToList();

            logger?.LogAlgorithmAction($"Bellman-Ford: {vertices.Count} vértices identificados para inicialização.");

            var result = new BellmanFordResult { Source = source };

            foreach (int vertexId in vertices)
            {
                result.Distances[vertexId] = double.PositiveInfinity;
                result.Predecessors[vertexId] = null;
                logger?.LogAlgorithmAction($"Bellman-Ford: distância de {vertexId} inicializada como INF.");
            }

            result.Distances[source] = 0;
            logger?.LogAlgorithmAction($"Bellman-Ford: distância da origem {source} definida como 0.");

            for (int i = 1; i <= vertices.Count - 1; i++)
            {
                logger?.LogAlgorithmAction($"Bellman-Ford: início da iteração {i} de {vertices.Count - 1}.");
                bool alters = false;

                foreach (var edge in graph.GetAllEdges())
                {
                    int v = edge.Source.GetHashCode();
                    int w = edge.Target.GetHashCode();
                    logger?.LogAlgorithmAction($"Bellman-Ford: avaliando aresta {v} -> {w} com custo {edge.LoadValue:0.###}.");

                    if (double.IsPositiveInfinity(result.Distances[v]))
                    {
                        logger?.LogAlgorithmAction($"Bellman-Ford: origem {v} ainda está em INF, aresta ignorada.");
                        continue;
                    }

                    double candidate = result.Distances[v] + edge.LoadValue;

                    if (result.Distances[w] > candidate)
                    {
                        logger?.LogAlgorithmAction($"Bellman-Ford: relaxamento aplicado em {w}. Distância anterior {result.Distances[w]:0.###}, nova {candidate:0.###} via {v}.");
                        result.Distances[w] = candidate;
                        result.Predecessors[w] = v;
                        alters = true;
                    }
                }

                if (!alters)
                {
                    logger?.LogAlgorithmAction($"Bellman-Ford: nenhuma alteração na iteração {i}; encerrando antecipadamente.");
                    break;
                }
            }

            logger?.LogAlgorithmAction("Bellman-Ford concluído.");

            return result;
        }

        public static void PrintResult(BellmanFordResult result)
        {
            Console.WriteLine("=== Caminho Mínimo com Bellman-Ford ===");
            Console.WriteLine($"Origem: {result.Source}");
            Console.WriteLine();

            foreach (var vertexId in result.Distances.Keys.OrderBy(id => id))
            {
                var distance = result.Distances[vertexId];
                string distanceText = double.IsPositiveInfinity(distance) ? "INF" : distance.ToString("0.###");
                string predecessorText = result.Predecessors[vertexId]?.ToString() ?? "null";

                Console.WriteLine($"Vértice {vertexId}: dist = {distanceText}, pred = {predecessorText}");
            }
        }
    }
}
