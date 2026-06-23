using graph_tp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace graph_tp.Algorithms
{
	/// <summary>
	/// Cenário A – Percurso de Rotas (Caminho/Circuito Euleriano).
	///
	/// Verifica se o inspetor consegue percorrer todas as rotas (arestas) exatamente
	/// uma vez. Caso seja possível um circuito (todos os vértices com grau par), o
	/// percurso retorna ao ponto de partida. Caso exista exatamente um par de vértices
	/// de grau ímpar, ainda há um caminho euleriano (sem retorno ao início).
	///
	/// A malha é tratada como NÃO direcionada: uma rota A->B pode ser inspecionada
	/// em qualquer sentido, então o que importa é o grau (número de rotas incidentes)
	/// de cada hub. Multigrafos (múltiplas rotas entre o mesmo par de hubs) são
	/// suportados, pois cada aresta é identificada individualmente.
	/// </summary>
	public class FleuryAlgorithm
	{
		// Representação interna de uma rota como aresta não direcionada, mantendo
		// um identificador único (para multigrafos) e a referência à aresta original.
		private class UndirectedRoute
		{
			public int Id { get; }
			public int U { get; }
			public int V { get; }
			public Edge Original { get; }

			public UndirectedRoute(int id, int u, int v, Edge original)
			{
				Id = id;
				U = u;
				V = v;
				Original = original;
			}

			// Dado um extremo conhecido, retorna o extremo oposto da rota.
			public int Other(int from) => from == U ? V : U;
		}

		public enum EulerianKind
		{
			None,    // Não existe percurso euleriano.
			Path,    // Existe caminho euleriano (não retorna à origem).
			Circuit  // Existe circuito euleriano (retorna à origem).
		}

		public class EulerianResult
		{
			public EulerianKind Kind { get; set; } = EulerianKind.None;

			// Sequência de hubs visitados, na ordem do percurso.
			public List<Vertex> VertexSequence { get; } = new List<Vertex>();

			// Rotas percorridas, na ordem em que foram atravessadas.
			public List<Edge> RouteSequence { get; } = new List<Edge>();

			// Justificativa textual quando o percurso não é possível.
			public string Reason { get; set; } = string.Empty;

			public bool IsPossible => Kind != EulerianKind.None;
		}

		public static EulerianResult RunFleury(Graph graph)
		{
			var result = new EulerianResult();

			// Constrói a representação não direcionada (cada rota vira uma aresta {u,v}).
			var routes = new List<UndirectedRoute>();
			var adjacency = new Dictionary<int, List<UndirectedRoute>>();

			foreach (var vertex in graph.GetAllvertexs())
				adjacency[vertex.GetHashCode()] = new List<UndirectedRoute>();

			int edgeId = 0;
			foreach (var edge in graph.GetAllEdges())
			{
				int u = edge.Source.GetHashCode();
				int v = edge.Target.GetHashCode();

				var route = new UndirectedRoute(edgeId++, u, v, edge);
				routes.Add(route);

				adjacency[u].Add(route);
				adjacency[v].Add(route);
			}

			if (routes.Count == 0)
			{
				result.Reason = "Não há rotas cadastradas para inspecionar.";
				return result;
			}

			// 1. Verifica os vértices de grau ímpar.
			//    - 0 ímpares  -> circuito euleriano (retorna à origem).
			//    - 2 ímpares  -> caminho euleriano (inicia em um ímpar, não retorna).
			//    - >= 3 ímpares (na prática 4, 6, ...) -> PARE: não existe percurso.
			var oddVertices = adjacency
				.Where(kv => kv.Value.Count % 2 != 0)
				.Select(kv => kv.Key)
				.ToList();

			if (oddVertices.Count >= 3)
			{
				result.Reason =
					$"Existem {oddVertices.Count} hubs com número ímpar de rotas incidentes. " +
					"Um percurso euleriano exige no máximo 2 hubs de grau ímpar.";
				return result;
			}

			// Verifica conectividade: todas as rotas precisam pertencer a um único
			// componente conexo (hubs isolados, sem rotas, são ignorados).
			if (!AllEdgesConnected(adjacency, routes))
			{
				result.Reason =
					"A malha de rotas é desconexa: não é possível percorrer todas as rotas " +
					"em um único trajeto contínuo.";
				return result;
			}

			result.Kind = oddVertices.Count == 0 ? EulerianKind.Circuit : EulerianKind.Path;

			// 2/3. Inicializa o grafo auxiliar G' e seleciona o vértice inicial.
			//      Preferencialmente um vértice de grau ímpar, se houver.
			var remaining = new HashSet<int>(routes.Select(r => r.Id));
			int current = oddVertices.Count > 0
				? oddVertices.First()
				: routes[0].U;

			result.VertexSequence.Add(graph.GetVertex(current)!);

			// 4. Enquanto houver arestas em G', caminhar escolhendo rotas que não sejam
			//    pontes (a menos que seja a única rota disponível no vértice atual).
			while (remaining.Count > 0)
			{
				var available = adjacency[current]
					.Where(r => remaining.Contains(r.Id))
					.ToList();

				if (available.Count == 0)
					break; // Segurança: não deveria ocorrer em grafo conexo válido.

				UndirectedRoute chosen;

				if (available.Count == 1)
				{
					// b) Única aresta disponível: caminhar por ela.
					chosen = available[0];
				}
				else
				{
					// a) Selecionar uma aresta que não seja ponte em G'.
					chosen = available.FirstOrDefault(r => !IsBridge(adjacency, remaining, current, r))
							 ?? available[0];
				}

				int next = chosen.Other(current);

				// c) Caminhar de v para w e remover a aresta de G'.
				remaining.Remove(chosen.Id);
				result.RouteSequence.Add(chosen.Original);
				result.VertexSequence.Add(graph.GetVertex(next)!);

				current = next;
			}

			return result;
		}

		// Verifica se todas as rotas estão em um único componente conexo, partindo
		// de um extremo qualquer e visitando os vértices que possuem rotas incidentes.
		private static bool AllEdgesConnected(
			Dictionary<int, List<UndirectedRoute>> adjacency,
			List<UndirectedRoute> routes)
		{
			int start = routes[0].U;
			var reachable = ReachableVertices(adjacency, new HashSet<int>(routes.Select(r => r.Id)), start);

			// Todo vértice que possui pelo menos uma rota incidente deve ser alcançável.
			return adjacency
				.Where(kv => kv.Value.Count > 0)
				.All(kv => reachable.Contains(kv.Key));
		}

		// Conta/retorna os vértices alcançáveis a partir de 'start' usando apenas as
		// rotas ainda presentes em G' (conjunto 'remaining').
		private static HashSet<int> ReachableVertices(
			Dictionary<int, List<UndirectedRoute>> adjacency,
			HashSet<int> remaining,
			int start)
		{
			var visited = new HashSet<int>();
			var stack = new Stack<int>();
			stack.Push(start);
			visited.Add(start);

			while (stack.Count > 0)
			{
				int v = stack.Pop();
				foreach (var route in adjacency[v])
				{
					if (!remaining.Contains(route.Id))
						continue;

					int w = route.Other(v);
					if (visited.Add(w))
						stack.Push(w);
				}
			}

			return visited;
		}

		// Uma rota {v,w} é ponte se sua remoção reduz o número de vértices alcançáveis
		// a partir de v, ou seja, ela é o único elo entre dois trechos da malha.
		private static bool IsBridge(
			Dictionary<int, List<UndirectedRoute>> adjacency,
			HashSet<int> remaining,
			int v,
			UndirectedRoute route)
		{
			int before = ReachableVertices(adjacency, remaining, v).Count;

			remaining.Remove(route.Id);
			int after = ReachableVertices(adjacency, remaining, v).Count;
			remaining.Add(route.Id);

			return after < before;
		}

		public static void PrintResult(EulerianResult result)
		{
			Console.WriteLine("=== Rota Única de Inspeção | Cenário A - Percurso de Rotas (Euleriano - Fleury) ===");

			if (!result.IsPossible)
			{
				Console.WriteLine("Não é possível percorrer todas as rotas exatamente uma vez.");
				Console.WriteLine($"Motivo: {result.Reason}");
				return;
			}

			if (result.Kind == EulerianKind.Circuit)
				Console.WriteLine("É possível um CIRCUITO euleriano: todas as rotas são percorridas uma única vez, retornando ao hub inicial.");
			else
				Console.WriteLine("É possível um CAMINHO euleriano: todas as rotas são percorridas uma única vez, mas o trajeto NÃO retorna ao hub inicial.");

			Console.WriteLine();
			Console.WriteLine("Sequência de hubs:");
			Console.WriteLine("  " + string.Join(" -> ", result.VertexSequence.Select(v => v.Name)));

			Console.WriteLine();
			Console.WriteLine($"Total de rotas percorridas: {result.RouteSequence.Count}");
		}
	}
}
