using graph_tp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace graph_tp.Algorithms
{
    internal class BellmanFordAlgorithm
    {
        public static void RunBellmanFord(Graph graph, int source)
        {
            int V = graph.VertexCount;
            int E = graph.EdgesCount;
            int[] distance = new int[V];

            // 1. Inicializa as distâncias a partir da origem para todos os outros vértices como INFINITO
            for (int i = 0; i < V; ++i)
                distance[i] = int.MaxValue;
            distance[source] = 0;

            // 2. Relaxamento de todas as arestas |V| - 1 vezes
            for (int i = 1; i < V; ++i)
            {
                for (int j = 0; j < E; ++j)
                {
                    int u = graph.Edges[j].Source;
                    int v = graph.Edges[j].Destination;
                    int weight = graph.Edges[j].Weight;

                    if (distance[u] != int.MaxValue && distance[u] + weight < distance[v])
                        distance[v] = distance[u] + weight;
                }
            }

            // 3. Verifica se há ciclos de peso negativo
            for (int j = 0; j < E; ++j)
            {
                int u = graph.Edges[j].Source;
                int v = graph.Edges[j].Destination;
                int weight = graph.Edges[j].Weight;

                if (distance[u] != int.MaxValue && distance[u] + weight < distance[v])
                {
                    Console.WriteLine("O grafo contém um ciclo de peso negativo!");
                    return;
                }
            }

        }
    }
}
