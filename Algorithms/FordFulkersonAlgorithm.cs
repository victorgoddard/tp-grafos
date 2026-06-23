using graph_tp.Models;
using graph_tp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace graph_tp.Algorithms
{
    public class FordFulkersonAlgorithm
    {
        private enum ResidualEdgeType
        {
            Direct,
            Reverse
        }

        public class MaxFlowResult
        {
            public double MaxFlow { get; set; }
            public Dictionary<(int Source, int Target), double> Flows { get; } = new Dictionary<(int Source, int Target), double>();
            public List<List<int>> AugmentingPaths { get; } = new List<List<int>>();
        }

        private static bool BreadthFirstSearch(
            double[,] capacity,
            double[,] flow,
            int s,
            int t,
            int[] parent,
            ResidualEdgeType[] parentType,
            int V,
            QueryLogger? logger = null)
        {
            bool[] visited = new bool[V];
            Queue<int> queue = new Queue<int>();

            queue.Enqueue(s);
            visited[s] = true;
            parent[s] = -1;
            logger?.LogAlgorithmAction($"Ford-Fulkerson: BFS iniciado a partir da fonte {s}.");

            while (queue.Count != 0)
            {
                int u = queue.Dequeue();
                logger?.LogAlgorithmAction($"Ford-Fulkerson: vértice {u} removido da fila.");

                for (int v = 0; v < V; v++)
                {
                    if (visited[v])
                        continue;

                    double forwardResidual = capacity[u, v] - flow[u, v];
                    if (forwardResidual > 0)
                    {
                        logger?.LogAlgorithmAction($"Ford-Fulkerson: aresta direta residual {u} -> {v} disponível com capacidade {forwardResidual:0.###}.");
                        parent[v] = u;
                        parentType[v] = ResidualEdgeType.Direct;

                        if (v == t)
                        {
                            return true;
                        }

                        queue.Enqueue(v);
                        visited[v] = true;
                        continue;
                    }

                    double reverseResidual = flow[v, u];
                    if (reverseResidual > 0)
                    {
                        logger?.LogAlgorithmAction($"Ford-Fulkerson: aresta reversa residual {u} <- {v} disponível com capacidade {reverseResidual:0.###}.");
                        parent[v] = u;
                        parentType[v] = ResidualEdgeType.Reverse;

                        if (v == t)
                        {
                            return true;
                        }

                        queue.Enqueue(v);
                        visited[v] = true;
                    }
                }
            }

            return false;
        }

        private static MaxFlowResult FordFulkersonCore(double[,] capacity, int s, int t, int V, QueryLogger? logger = null)
        {
            var result = new MaxFlowResult();
            double[,] flow = new double[V, V];
            int[] parent = new int[V];
            ResidualEdgeType[] parentType = new ResidualEdgeType[V];

            logger?.LogAlgorithmAction($"Ford-Fulkerson: execução principal iniciada entre {s} e {t}.");

            while (BreadthFirstSearch(capacity, flow, s, t, parent, parentType, V, logger))
            {
                double pathFlow = double.PositiveInfinity;
                logger?.LogAlgorithmAction("Ford-Fulkerson: caminho aumentante encontrado, calculando gargalo.");

                for (int v = t; v != s; v = parent[v])
                {
                    int u = parent[v];

                    double residualCapacity = parentType[v] == ResidualEdgeType.Direct
                        ? capacity[u, v] - flow[u, v]
                        : flow[v, u];

                    pathFlow = Math.Min(pathFlow, residualCapacity);
                }

                logger?.LogAlgorithmAction($"Ford-Fulkerson: gargalo do caminho aumentante = {pathFlow:0.###}.");

                var pathVertices = new List<int> { t };

                for (int v = t; v != s; v = parent[v])
                {
                    int u = parent[v];

                    if (parentType[v] == ResidualEdgeType.Direct)
                    {
                        flow[u, v] += pathFlow;
                        logger?.LogAlgorithmAction($"Ford-Fulkerson: fluxo direto atualizado em {u} -> {v} para {flow[u, v]:0.###}.");
                    }
                    else
                    {
                        flow[v, u] -= pathFlow;
                        logger?.LogAlgorithmAction($"Ford-Fulkerson: fluxo reverso atualizado em {v} -> {u} para {flow[v, u]:0.###}.");
                    }

                    pathVertices.Add(u);
                }

                pathVertices.Reverse();
                result.AugmentingPaths.Add(pathVertices);
                result.MaxFlow += pathFlow;
                logger?.LogAlgorithmAction($"Ford-Fulkerson: caminho {string.Join(" -> ", pathVertices)} aplicado; fluxo total = {result.MaxFlow:0.###}.");
            }

            for (int u = 0; u < V; u++)
            {
                for (int v = 0; v < V; v++)
                {
                    if (flow[u, v] > 0)
                        result.Flows[(u, v)] = flow[u, v];
                }
            }

            logger?.LogAlgorithmAction($"Ford-Fulkerson concluído com fluxo máximo {result.MaxFlow:0.###}.");

            return result;
        }

        public static MaxFlowResult FordFulkerson(Graph graph, int source, int sink, QueryLogger? logger = null)
        {
            if (!graph.Containsvertex(source))
                throw new ArgumentException($"Vértice fonte {source} não existe no grafo.");

            if (!graph.Containsvertex(sink))
                throw new ArgumentException($"Vértice sorvedouro {sink} não existe no grafo.");

            var vertices = graph.GetAllvertexs()
                .Select(vertex => vertex.GetHashCode())
                .ToList();

            int vertexCount = vertices.Count;
            var indexOf = new Dictionary<int, int>(vertexCount);

            for (int i = 0; i < vertexCount; i++)
                indexOf[vertices[i]] = i;

            logger?.LogAlgorithmAction($"Ford-Fulkerson: mapeados {vertexCount} vértices para a matriz residual.");

            double[,] capacity = new double[vertexCount, vertexCount];

            foreach (var edge in graph.GetAllEdges())
            {
                if (double.IsPositiveInfinity(edge.Capacity))
                    throw new InvalidOperationException("Ford-Fulkerson requer capacidades finitas em todas as arestas.");

                int u = indexOf[edge.Source.GetHashCode()];
                int v = indexOf[edge.Target.GetHashCode()];
                capacity[u, v] += edge.Capacity;
                logger?.LogAlgorithmAction($"Ford-Fulkerson: capacidade registrada {u} -> {v} = {capacity[u, v]:0.###}.");
            }

            int s = indexOf[source];
            int t = indexOf[sink];
            return FordFulkersonCore(capacity, s, t, vertexCount, logger);
        }

        public static int FordFulkerson(int[,] graph, int s, int t, int V, QueryLogger? logger = null)
        {
            double[,] capacity = new double[V, V];

            for (int u = 0; u < V; u++)
                for (int v = 0; v < V; v++)
                    capacity[u, v] = graph[u, v];

            var result = FordFulkersonCore(capacity, s, t, V, logger);
            return (int)Math.Round(result.MaxFlow);
        }

        public static void PrintResult(MaxFlowResult result)
        {
            Console.WriteLine("=== Fluxo Maximo com Ford-Fulkerson ===");
            Console.WriteLine($"Fluxo maximo: {result.MaxFlow:0.###}");
            Console.WriteLine();

            if (result.AugmentingPaths.Count > 0)
            {
                Console.WriteLine("Caminhos aumentantes:");
                for (int i = 0; i < result.AugmentingPaths.Count; i++)
                {
                    Console.WriteLine($"  {i + 1}. {string.Join(" -> ", result.AugmentingPaths[i])}");
                }
                Console.WriteLine();
            }

            if (result.Flows.Count > 0)
            {
                Console.WriteLine("Fluxos nas arestas:");
                foreach (var entry in result.Flows.OrderBy(entry => entry.Key.Source).ThenBy(entry => entry.Key.Target))
                {
                    Console.WriteLine($"  {entry.Key.Source} -> {entry.Key.Target}: {entry.Value:0.###}");
                }
            }
        }
    }
}
