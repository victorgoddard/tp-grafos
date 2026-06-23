using System.Collections.Generic;
using graph_tp.Models;

namespace graph_tp.Models.Representations
{
    public enum RepresentationKind
    {
        AdjacencyList,
        AdjacencyMatrix,
        IncidenceMatrix
    }

    public interface IGraphRepresentation
    {
        int VertexCount { get; }
        int EdgeCount { get; }

        void AddVertex(Vertex vertex);
        void AddEdge(Edge edge);

        Vertex? GetVertex(int id);
        List<Edge> GetOutgoingEdges(int vertexId);
        List<Edge> GetIncomingEdges(int vertexId);
        IEnumerable<Vertex> GetAllVertices();
        IEnumerable<Edge> GetAllEdges();
        bool ContainsVertex(int vertexId);
    }
}
