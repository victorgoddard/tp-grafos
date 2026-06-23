using graph_tp.Models;
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

		public static MinimumSpanningTree RunPrim(Graph graph, int? root = null)
		{
			var result = new MinimumSpanningTree();
			var allVertexIds = graph.GetAllvertexs()
									.Select(v => v.GetHashCode())
									.ToList();

			if (allVertexIds.Count == 0)
			{
				result.IsConnected = true;
				return result;
			}

			int r = root ?? allVertexIds.First();

			if (!graph.Containsvertex(r))
				throw new ArgumentException($"Vértice raiz {r} não existe no grafo.");

			var inTree = new HashSet<int> { r };

			while (inTree.Count != allVertexIds.Count)
			{
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

					if (minEdge == null || edge.LoadValue < minEdge.LoadValue)
						minEdge = edge;
				}

				if (minEdge == null)
				{
					result.IsConnected = false;
					return result;
				}

				int newVertexId = inTree.Contains(minEdge.Source.GetHashCode())
					? minEdge.Target.GetHashCode()
					: minEdge.Source.GetHashCode();

				inTree.Add(newVertexId);

				result.Edges.Add(minEdge);
				result.TotalCost += minEdge.LoadValue;
			}

			result.IsConnected = true;
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
