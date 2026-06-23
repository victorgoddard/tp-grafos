using graph_tp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace graph_tp.Algorithms
{
	public class WelshPowellAlgorithm
	{
		private class RouteNode
		{
			public int Index { get; }
			public Edge Route { get; }
			public int Degree { get; set; }
			public int Color { get; set; } = -1;

			public RouteNode(int index, Edge route)
			{
				Index = index;
				Route = route;
			}
		}

		public class MaintenanceSchedule
		{
			public Dictionary<int, List<Edge>> Shifts { get; } = new Dictionary<int, List<Edge>>();
			public int TotalShifts => Shifts.Count;
		}

		public static MaintenanceSchedule RunWelshPowell(Graph graph)
		{
			var result = new MaintenanceSchedule();
			var nodes = graph.GetAllEdges()
							 .Select((edge, index) => new RouteNode(index, edge))
							 .ToList();

			if (nodes.Count == 0)
				return result;

			bool[,] conflict = new bool[nodes.Count, nodes.Count];

			for (int i = 0; i < nodes.Count; i++)
			{
				for (int j = i + 1; j < nodes.Count; j++)
				{
					if (RoutesShareHub(nodes[i].Route, nodes[j].Route))
					{
						conflict[i, j] = true;
						conflict[j, i] = true;
						nodes[i].Degree++;
						nodes[j].Degree++;
					}
				}
			}


			var ordered = nodes.OrderByDescending(n => n.Degree).ToList();
			int currentColor = 0;
			int coloredCount = 0;

			while (coloredCount < ordered.Count)
			{
				currentColor++;

				foreach (var node in ordered)
				{
					if (node.Color != -1)
						continue;

					bool hasConflictWithColor = ordered.Any(other =>
						other.Color == currentColor &&
						conflict[node.Index, other.Index]);

					if (!hasConflictWithColor)
					{
						node.Color = currentColor;
						coloredCount++;
					}
				}
			}


			foreach (var node in ordered.OrderBy(n => n.Index))
			{
				if (!result.Shifts.ContainsKey(node.Color))
					result.Shifts[node.Color] = new List<Edge>();

				result.Shifts[node.Color].Add(node.Route);
			}

			return result;
		}
		private static bool RoutesShareHub(Edge a, Edge b)
		{
			int aSource = a.Source.GetHashCode();
			int aTarget = a.Target.GetHashCode();
			int bSource = b.Source.GetHashCode();
			int bTarget = b.Target.GetHashCode();

			return aSource == bSource || aSource == bTarget ||
				   aTarget == bSource || aTarget == bTarget;
		}

		public static void PrintResult(MaintenanceSchedule schedule)
		{
			Console.WriteLine("=== Agendamento de Manutenções sem Conflito (Welsh-Powell) ===");

			if (schedule.TotalShifts == 0)
			{
				Console.WriteLine("Nenhuma rota cadastrada para manutenção.");
				return;
			}

			Console.WriteLine($"Número mínimo de turnos necessários: {schedule.TotalShifts}");
			Console.WriteLine();

			foreach (var shift in schedule.Shifts.OrderBy(s => s.Key))
			{
				Console.WriteLine($"Turno {shift.Key}:");
				foreach (var route in shift.Value)
				{
					Console.WriteLine($"  {route.Source.Name} -> {route.Target.Name}");
				}
			}
		}
	}
}
