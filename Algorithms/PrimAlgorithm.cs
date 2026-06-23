using graph_tp.Models;
using graph_tp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace graph_tp.Algorithms
{
	public class PrimAlgorithm
	{
		public class MinimumSpanningTree
		{
			public List<Edge> Edges { get; } = new List<Edge>();
			public double TotalCost { get; set; }
			public bool IsConnected { get; set; }
		}

		public static MinimumSpanningTree RunPrim(Graph graph, int? root = null, QueryLogger? logger = null)
		{
			var result = new MinimumSpanningTree();
			var allVertexIds = graph.GetAllvertexs()
									.Select(v => v.GetHashCode())
									.ToList();

			logger?.LogAlgorithmAction($"Prim iniciado com {allVertexIds.Count} vértices.");

			if (allVertexIds.Count == 0)
			{
				result.IsConnected = true;
				logger?.LogAlgorithmAction("Prim: grafo vazio, MST trivial concluída.");
				return result;
			}

			int r = root ?? allVertexIds.First();

			if (!graph.Containsvertex(r))
				throw new ArgumentException($"Vértice raiz {r} não existe no grafo.");

			logger?.LogAlgorithmAction($"Prim: raiz definida como {r}.");

			var inTree = new HashSet<int> { r };

			while (inTree.Count != allVertexIds.Count)
			{
				logger?.LogAlgorithmAction($"Prim: árvore parcial possui {inTree.Count}/{allVertexIds.Count} vértices.");
				Edge? minEdge = null;

				foreach (var edge in graph.GetAllEdges())
				{
					int sourceId = edge.Source.GetHashCode();
					int targetId = edge.Target.GetHashCode();

					bool sourceInTree = inTree.Contains(sourceId);
					bool targetInTree = inTree.Contains(targetId);
					bool crossesCut = sourceInTree ^ targetInTree;

					if (!crossesCut)
						continue;

					logger?.LogAlgorithmAction($"Prim: aresta candidata {sourceId} -- {targetId} com custo {edge.LoadValue:0.###} cruzando o corte.");

					if (minEdge == null || edge.LoadValue < minEdge.LoadValue)
					{
						minEdge = edge;
						logger?.LogAlgorithmAction($"Prim: aresta {sourceId} -- {targetId} torna-se a melhor candidata no momento.");
					}
				}

				if (minEdge == null)
				{
					result.IsConnected = false;
					logger?.LogAlgorithmAction("Prim: nenhuma aresta cruzando o corte foi encontrada; grafo desconexo.");
					return result;
				}

				int newVertexId = inTree.Contains(minEdge.Source.GetHashCode())
					? minEdge.Target.GetHashCode()
					: minEdge.Source.GetHashCode();

				inTree.Add(newVertexId);

				result.Edges.Add(minEdge);
				result.TotalCost += minEdge.LoadValue;
				logger?.LogAlgorithmAction($"Prim: aresta selecionada {minEdge.Source.GetHashCode()} -- {minEdge.Target.GetHashCode()} com custo {minEdge.LoadValue:0.###}. Novo vértice incluído: {newVertexId}.");
			}

			result.IsConnected = true;
			logger?.LogAlgorithmAction($"Prim concluído com custo total {result.TotalCost:0.###}.");
			return result;
		}


		public static void PrintResult(MinimumSpanningTree mst)
		{
			Console.WriteLine("=== Expansão da Rede de Comunicação (AGM - Prim) ===");

			if (!mst.IsConnected)
			{
				Console.WriteLine("O grafo é desconexo: não é possível interligar todos os hubs.");
				Console.WriteLine("Árvore parcial encontrada:");
			}

			if (mst.Edges.Count == 0)
			{
				Console.WriteLine("Nenhuma rota selecionada.");
				return;
			}

			Console.WriteLine("Rotas selecionadas para a rede:");
			foreach (var edge in mst.Edges)
			{
				Console.WriteLine($"  {edge.Source.Name} -- {edge.Target.Name} | custo: R$ {edge.LoadValue}");
			}

			Console.WriteLine($"Custo total de instalação: R$ {mst.TotalCost}");
		}
	}
}
