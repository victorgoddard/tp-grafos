using graph_tp.Models;
using graph_tp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace graph_tp.Algorithms
{
	public class FleuryAlgorithm
	{
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
			public int Other(int from) => from == U ? V : U;
		}

		public enum EulerianKind
		{
			None,
			Path,
			Circuit
		}

		public class EulerianResult
		{
			public EulerianKind Kind { get; set; } = EulerianKind.None;
			public List<Vertex> VertexSequence { get; } = new List<Vertex>();
			public List<Edge> RouteSequence { get; } = new List<Edge>();
			public string Reason { get; set; } = string.Empty;
			public bool IsPossible => Kind != EulerianKind.None;
		}

		public static EulerianResult RunFleury(Graph graph, QueryLogger? logger = null)
		{
			var result = new EulerianResult();
			logger?.LogAlgorithmAction("Fleury iniciado.");

			var routes = new List<UndirectedRoute>();
			var adjacency = new Dictionary<int, List<UndirectedRoute>>();

			foreach (var vertex in graph.GetAllvertexs())
				adjacency[vertex.GetHashCode()] = new List<UndirectedRoute>();
			logger?.LogAlgorithmAction($"Fleury: {adjacency.Count} vértices carregados na estrutura auxiliar.");

			int edgeId = 0;
			foreach (var edge in graph.GetAllEdges())
			{
				int u = edge.Source.GetHashCode();
				int v = edge.Target.GetHashCode();

				var route = new UndirectedRoute(edgeId++, u, v, edge);
				routes.Add(route);

				adjacency[u].Add(route);
				adjacency[v].Add(route);
				logger?.LogAlgorithmAction($"Fleury: rota registrada {u} <-> {v}.");
			}

			if (routes.Count == 0)
			{
				result.Reason = "Não há rotas cadastradas para inspecionar.";
				logger?.LogAlgorithmAction("Fleury: nenhuma rota disponível.");
				return result;
			}

			var oddVertices = adjacency
				.Where(kv => kv.Value.Count % 2 != 0)
				.Select(kv => kv.Key)
				.ToList();

			if (oddVertices.Count >= 3)
			{
				result.Reason =
					$"Existem {oddVertices.Count} hubs com número ímpar de rotas incidentes. " +
					"Um percurso euleriano exige no máximo 2 hubs de grau ímpar.";
				logger?.LogAlgorithmAction($"Fleury: falha por {oddVertices.Count} vértices de grau ímpar.");
				return result;
			}
			logger?.LogAlgorithmAction($"Fleury: vértices de grau ímpar = {oddVertices.Count}.");

			if (!AllEdgesConnected(adjacency, routes))
			{
				result.Reason =
					"A malha de rotas é desconexa: não é possível percorrer todas as rotas " +
					"em um único trajeto contínuo.";
				logger?.LogAlgorithmAction("Fleury: grafo desconexo, encerrando.");
				return result;
			}

			result.Kind = oddVertices.Count == 0 ? EulerianKind.Circuit : EulerianKind.Path;

			var remaining = new HashSet<int>(routes.Select(r => r.Id));
			int current = oddVertices.Count > 0
				? oddVertices.First()
				: routes[0].U;
			logger?.LogAlgorithmAction($"Fleury: vértice inicial definido como {current}.");

			result.VertexSequence.Add(graph.GetVertex(current)!);

			while (remaining.Count > 0)
			{
				logger?.LogAlgorithmAction($"Fleury: {remaining.Count} arestas restantes.");
				var available = adjacency[current]
					.Where(r => remaining.Contains(r.Id))
					.ToList();

				if (available.Count == 0)
					break;

				UndirectedRoute chosen;

				if (available.Count == 1)
				{
					chosen = available[0];
					logger?.LogAlgorithmAction($"Fleury: única aresta disponível em {current} é {chosen.U} <-> {chosen.V}.");
				}
				else
				{
					chosen = available.FirstOrDefault(r => !IsBridge(adjacency, remaining, current, r))
							 ?? available[0];
					logger?.LogAlgorithmAction($"Fleury: aresta escolhida em {current} = {chosen.U} <-> {chosen.V}.");
				}

				int next = chosen.Other(current);

				remaining.Remove(chosen.Id);
				result.RouteSequence.Add(chosen.Original);
				result.VertexSequence.Add(graph.GetVertex(next)!);
				logger?.LogAlgorithmAction($"Fleury: deslocamento {current} -> {next} executado.");

				current = next;
			}

			logger?.LogAlgorithmAction($"Fleury concluído com {result.RouteSequence.Count} rotas percorridas.");

			return result;
		}

		private static bool AllEdgesConnected(
			Dictionary<int, List<UndirectedRoute>> adjacency,
			List<UndirectedRoute> routes)
		{
			int start = routes[0].U;
			var reachable = ReachableVertices(adjacency, new HashSet<int>(routes.Select(r => r.Id)), start);

			return adjacency
				.Where(kv => kv.Value.Count > 0)
				.All(kv => reachable.Contains(kv.Key));
		}

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
