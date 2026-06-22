using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace graph_tp.Algorithms
{
    public class FordFulkersonAlgorithm
    {
        // Retorna verdadeiro se existir um caminho da fonte (s) para o sorvedouro (t)
        // no grafo residual. Também preenche a matriz parent[] para armazenar o caminho.
        private static bool BreadthFirstSearch(int[,] rGraph, int s, int t, int[] parent, int V)
        {
            bool[] visited = new bool[V];
            for (int i = 0; i < V; ++i)
                visited[i] = false;

            Queue<int> queue = new Queue<int>();
            queue.Enqueue(s);
            visited[s] = true;
            parent[s] = -1;

            while (queue.Count != 0)
            {
                int u = queue.Dequeue();

                for (int v = 0; v < V; v++)
                {
                    if (visited[v] == false && rGraph[u, v] > 0)
                    {
                        if (v == t)
                        {
                            parent[v] = u;
                            return true;
                        }
                        queue.Enqueue(v);
                        parent[v] = u;
                        visited[v] = true;
                    }
                }
            }
            return false;
        }

        // Retorna o fluxo máximo de s para t no grafo dado
        public static int FordFulkerson(int[,] graph, int s, int t, int V)
        {
            int u, v;

            // Grafo residual
            int[,] rGraph = new int[V, V];

            for (u = 0; u < V; u++)
                for (v = 0; v < V; v++)
                    rGraph[u, v] = graph[u, v];

            int[] parent = new int[V];
            int maxFlow = 0;

            // Aumenta o fluxo enquanto houver um caminho da fonte para o sorvedouro
            while (BreadthFirstSearch(rGraph, s, t, parent, V))
            {
                // Encontra a capacidade residual mínima das arestas ao longo do caminho preenchido pelo BFS
                int pathFlow = int.MaxValue;
                for (v = t; v != s; v = parent[v])
                {
                    u = parent[v];
                    pathFlow = Math.Min(pathFlow, rGraph[u, v]);
                }

                // Atualiza as capacidades residuais das arestas e arestas reversas
                for (v = t; v != s; v = parent[v])
                {
                    u = parent[v];
                    rGraph[u, v] -= pathFlow;
                    rGraph[v, u] += pathFlow;
                }

                maxFlow += pathFlow;
            }

            return maxFlow;
        }

    }
}
