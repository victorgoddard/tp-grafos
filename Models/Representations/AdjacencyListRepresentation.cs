using System.Collections.Generic;
using System.Linq;
using graph_tp.Models;

namespace graph_tp.Models.Representations
{
    public class AdjacencyListRepresentation : IGraphRepresentation
    {
        private readonly Dictionary<int, Vertex> _vertices = new();
        private readonly Dictionary<int, List<Edge>> _outgoing = new();

        public int VertexCount => _vertices.Count;
        public int EdgeCount => _outgoing.Values.Sum(edges => edges.Count);

        public void AddVertex(Vertex vertex)
        {
            int id = vertex.GetHashCode();
            if (!_vertices.ContainsKey(id))
            {
                _vertices[id] = vertex;
                _outgoing[id] = new List<Edge>();
            }
        }

        public void AddEdge(Edge edge)
        {
            AddVertex(edge.Source);
            AddVertex(edge.Target);
            _outgoing[edge.Source.GetHashCode()].Add(edge);
        }

        public Vertex? GetVertex(int id)
            => _vertices.TryGetValue(id, out var v) ? v : null;

        public List<Edge> GetOutgoingEdges(int vertexId)
            => _outgoing.TryGetValue(vertexId, out var edges)
                ? new List<Edge>(edges)
                : new List<Edge>();

        public List<Edge> GetIncomingEdges(int vertexId)
        {
            var incoming = new List<Edge>();
            foreach (var edges in _outgoing.Values)
                incoming.AddRange(edges.Where(e => e.Target.GetHashCode() == vertexId));
            return incoming;
        }

        public IEnumerable<Vertex> GetAllVertices() => _vertices.Values;

        public IEnumerable<Edge> GetAllEdges() => _outgoing.Values.SelectMany(e => e);

        public bool ContainsVertex(int vertexId) => _vertices.ContainsKey(vertexId);
    }
}
