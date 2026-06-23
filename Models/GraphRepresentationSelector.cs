using System;
using System.Globalization;
using graph_tp.Models.Representations;

namespace graph_tp.Models
{
    public static class GraphRepresentationSelector
    {
        public const double DenseThreshold = 0.5;

        // Densidade de grafo direcionado: D = E / (V * (V - 1)). V < 2 => 0.
        public static double Density(int vertexCount, int edgeCount)
        {
            if (vertexCount < 2) return 0.0;
            return (double)edgeCount / ((double)vertexCount * (vertexCount - 1));
        }

        public static RepresentationKind Select(int vertexCount, int edgeCount)
        {
            if (vertexCount >= 2 && Density(vertexCount, edgeCount) >= DenseThreshold)
                return RepresentationKind.AdjacencyMatrix;
            return RepresentationKind.AdjacencyList;
        }

        public static IGraphRepresentation Create(RepresentationKind kind) => kind switch
        {
            RepresentationKind.AdjacencyMatrix => new AdjacencyMatrixRepresentation(),
            RepresentationKind.IncidenceMatrix => new IncidenceMatrixRepresentation(),
            _ => new AdjacencyListRepresentation(),
        };

        public static string Report(int vertexCount, int edgeCount, RepresentationKind chosen)
        {
            double d = Density(vertexCount, edgeCount);
            string pct = (d * 100).ToString("F2", CultureInfo.InvariantCulture);
            string classification = d >= DenseThreshold ? "denso" : "esparso";

            return
                $"Vértices (V): {vertexCount}{Environment.NewLine}" +
                $"Arestas (E): {edgeCount}{Environment.NewLine}" +
                $"Densidade D = E / (V*(V-1)) = {pct}% (limiar {(DenseThreshold * 100):F0}% => {classification}){Environment.NewLine}" +
                $"Custo de espaço: lista O(n+m) | matriz adj. O(n^2) | matriz inc. O(n*m){Environment.NewLine}" +
                $"Estrutura selecionada: {chosen}";
        }
    }
}
