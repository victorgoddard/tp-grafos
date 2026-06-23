using graph_tp.Models;
using graph_tp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace graph_tp.Algorithms
{
	public class HamiltonAlgorithm
	{
		public class HamiltonianResult
		{
			public bool IsPossible { get; set; }
			public List<Vertex> Cycle { get; } = new List<Vertex>();
			public string Reason { get; set; } = string.Empty;
		}

		public static HamiltonianResult RunHamiltonian(Graph graph, QueryLogger? logger = null)
		{
			var result = new HamiltonianResult();
			logger?.LogAlgorithmAction("Hamiltoniano iniciado.");

			var vertexIds = graph.GetAllvertexs().Select(v => v.GetHashCode()).ToList();
			int n = vertexIds.Count;
			logger?.LogAlgorithmAction($"Hamiltoniano: {n} vértices identificados.");

			if (n == 0)
			{
				result.Reason = "Não há hubs cadastrados.";
				logger?.LogAlgorithmAction("Hamiltoniano: grafo vazio.");
				return result;
			}

			if (n == 1)
			{
				result.IsPossible = true;
				var only = graph.GetVertex(vertexIds[0])!;
				result.Cycle.Add(only);
				result.Cycle.Add(only);
				logger?.LogAlgorithmAction("Hamiltoniano: caso trivial com um único vértice.");
				return result;
			}

			var indexOf = new Dictionary<int, int>();
			for (int i = 0; i < n; i++)
				indexOf[vertexIds[i]] = i;

			bool[,] adjacent = new bool[n, n];
			foreach (var edge in graph.GetAllEdges())
			{
				int u = indexOf[edge.Source.GetHashCode()];
				int v = indexOf[edge.Target.GetHashCode()];
				if (u == v)
					continue;

				adjacent[u, v] = true;
				adjacent[v, u] = true;
				logger?.LogAlgorithmAction($"Hamiltoniano: adjacência registrada entre {vertexIds[u]} e {vertexIds[v]}.");
			}

			var path = new int[n];
			var used = new bool[n];

			path[0] = 0;
			used[0] = true;
			logger?.LogAlgorithmAction($"Hamiltoniano: backtracking iniciado a partir do vértice {vertexIds[0]}.");

			if (Solve(adjacent, path, used, n, 1, vertexIds, logger))
			{
				result.IsPossible = true;
				foreach (int idx in path)
					result.Cycle.Add(graph.GetVertex(vertexIds[idx])!);

				result.Cycle.Add(graph.GetVertex(vertexIds[path[0]])!);
				logger?.LogAlgorithmAction("Hamiltoniano: ciclo encontrado com sucesso.");
			}
			else
			{
				result.Reason =
					"Não existe um ciclo que visite todos os hubs exatamente uma vez " +
					"retornando ao ponto de partida (grafo sem ciclo hamiltoniano).";
				logger?.LogAlgorithmAction("Hamiltoniano: nenhuma solução encontrada.");
			}

			return result;
		}

		private static bool Solve(bool[,] adjacent, int[] path, bool[] used, int n, int pos, List<int> vertexIds, QueryLogger? logger)
		{
			if (pos == n)
			{
				// Todos os hubs visitados: o ciclo só é válido se o último retornar ao primeiro.
				logger?.LogAlgorithmAction($"Hamiltoniano: verificação final entre {vertexIds[path[pos - 1]]} e {vertexIds[path[0]]}.");
				return adjacent[path[pos - 1], path[0]];
			}

			for (int candidate = 0; candidate < n; candidate++)
			{
				if (used[candidate])
					continue;

				if (!adjacent[path[pos - 1], candidate])
					continue;

				logger?.LogAlgorithmAction($"Hamiltoniano: candidato {vertexIds[candidate]} testado na posição {pos}.");

				path[pos] = candidate;
				used[candidate] = true;

				if (Solve(adjacent, path, used, n, pos + 1, vertexIds, logger))
				{
					logger?.LogAlgorithmAction($"Hamiltoniano: candidato {vertexIds[candidate]} confirmado na posição {pos}.");
					return true;
				}

				used[candidate] = false;
				logger?.LogAlgorithmAction($"Hamiltoniano: retrocedendo do candidato {vertexIds[candidate]} na posição {pos}.");
			}

			return false;
		}

		public static void PrintResult(HamiltonianResult result)
		{
			Console.WriteLine("=== Rota Única de Inspeção | Cenário B - Percurso de Hubs (Ciclo Hamiltoniano) ===");

			if (!result.IsPossible)
			{
				Console.WriteLine("Não é possível visitar todos os hubs exatamente uma vez retornando ao início.");
				Console.WriteLine($"Motivo: {result.Reason}");
				return;
			}

			Console.WriteLine("É possível visitar todos os hubs exatamente uma vez e retornar ao hub inicial.");
			Console.WriteLine();
			Console.WriteLine("Sequência de hubs:");
			Console.WriteLine("  " + string.Join(" -> ", result.Cycle.Select(v => v.Name)));
		}
	}
}
