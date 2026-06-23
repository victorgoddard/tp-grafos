using graph_tp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace graph_tp.Algorithms
{
	/// <summary>
	/// Cenário B – Percurso de Hubs (Ciclo Hamiltoniano).
	///
	/// Verifica se o inspetor consegue visitar todos os hubs (vértices) exatamente
	/// uma vez, retornando ao hub inicial. Este é o problema do Ciclo Hamiltoniano,
	/// que é NP-completo: não há algoritmo eficiente conhecido. A solução exata é
	/// obtida por backtracking — construímos o trajeto hub a hub, retrocedendo
	/// sempre que um caminho parcial não puder ser estendido.
	///
	/// A malha é tratada como NÃO direcionada (mesma premissa do Cenário A): uma
	/// rota A->B permite o deslocamento do inspetor em qualquer sentido.
	/// </summary>
	public class HamiltonAlgorithm
	{
		public class HamiltonianResult
		{
			public bool IsPossible { get; set; }

			// Sequência de hubs do ciclo (o primeiro hub repete-se ao final, fechando o ciclo).
			public List<Vertex> Cycle { get; } = new List<Vertex>();

			public string Reason { get; set; } = string.Empty;
		}

		public static HamiltonianResult RunHamiltonian(Graph graph)
		{
			var result = new HamiltonianResult();

			var vertexIds = graph.GetAllvertexs().Select(v => v.GetHashCode()).ToList();
			int n = vertexIds.Count;

			if (n == 0)
			{
				result.Reason = "Não há hubs cadastrados.";
				return result;
			}

			if (n == 1)
			{
				// Um único hub: trivialmente "visitado", ciclo degenerado.
				result.IsPossible = true;
				var only = graph.GetVertex(vertexIds[0])!;
				result.Cycle.Add(only);
				result.Cycle.Add(only);
				return result;
			}

			// Mapeia cada id de vértice para um índice contíguo [0..n-1].
			var indexOf = new Dictionary<int, int>();
			for (int i = 0; i < n; i++)
				indexOf[vertexIds[i]] = i;

			// Matriz de adjacência não direcionada entre hubs.
			bool[,] adjacent = new bool[n, n];
			foreach (var edge in graph.GetAllEdges())
			{
				int u = indexOf[edge.Source.GetHashCode()];
				int v = indexOf[edge.Target.GetHashCode()];
				if (u == v)
					continue; // laços não contribuem para o ciclo hamiltoniano.

				adjacent[u, v] = true;
				adjacent[v, u] = true;
			}

			// Backtracking iniciando sempre pelo hub 0 (qualquer hub serve como início,
			// pois um ciclo hamiltoniano pode ser "rotacionado" para começar em qualquer vértice).
			var path = new int[n];
			var used = new bool[n];

			path[0] = 0;
			used[0] = true;

			if (Solve(adjacent, path, used, n, 1))
			{
				result.IsPossible = true;
				foreach (int idx in path)
					result.Cycle.Add(graph.GetVertex(vertexIds[idx])!);

				// Fecha o ciclo retornando ao hub inicial.
				result.Cycle.Add(graph.GetVertex(vertexIds[path[0]])!);
			}
			else
			{
				result.Reason =
					"Não existe um ciclo que visite todos os hubs exatamente uma vez " +
					"retornando ao ponto de partida (grafo sem ciclo hamiltoniano).";
			}

			return result;
		}

		// Tenta posicionar um hub na posição 'pos' do trajeto. Quando todos os hubs
		// foram posicionados, verifica se o último se conecta de volta ao inicial.
		private static bool Solve(bool[,] adjacent, int[] path, bool[] used, int n, int pos)
		{
			if (pos == n)
			{
				// Todos os hubs visitados: o ciclo só é válido se o último retornar ao primeiro.
				return adjacent[path[pos - 1], path[0]];
			}

			for (int candidate = 0; candidate < n; candidate++)
			{
				if (used[candidate])
					continue;

				// O candidato precisa ser adjacente ao último hub do trajeto parcial.
				if (!adjacent[path[pos - 1], candidate])
					continue;

				path[pos] = candidate;
				used[candidate] = true;

				if (Solve(adjacent, path, used, n, pos + 1))
					return true;

				// Retrocede: o candidato não levou a uma solução.
				used[candidate] = false;
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
