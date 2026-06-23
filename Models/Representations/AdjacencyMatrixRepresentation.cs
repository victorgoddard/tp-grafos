using System.Collections.Generic;
using graph_tp.Models;

namespace graph_tp.Models.Representations
{
    public class AdjacencyMatrixRepresentation : IGraphRepresentation
    {
        private readonly List<Vertex> _vertices = new();
        private readonly Dictionary<int, Vertex> _byId = new();
        private readonly List<Edge> _edges = new();

        private Dictionary<int, int> _idToIndex = new();
        private List<Edge>[,]? _matrix;
        private bool _dirty = true;

        public int VertexCount => _vertices.Count;
        public int EdgeCount => _edges.Count;

        public void AddVertex(Vertex vertex)
        {
            int id = vertex.GetHashCode();
            if (_byId.ContainsKey(id)) return;
            _byId[id] = vertex;
            _vertices.Add(vertex);
            _dirty = true;
        }

        public void AddEdge(Edge edge)
        {
            AddVertex(edge.Source);
            AddVertex(edge.Target);
            _edges.Add(edge);
            _dirty = true;
        }

        private void EnsureBuilt()
        {
            if (!_dirty) return;

            int n = _vertices.Count;
            _idToIndex = new Dictionary<int, int>(n);
            for (int i = 0; i < n; i++)
                _idToIndex[_vertices[i].GetHashCode()] = i;

            _matrix = new List<Edge>[n, n];
            foreach (var e in _edges)
            {
                int u = _idToIndex[e.Source.GetHashCode()];
                int v = _idToIndex[e.Target.GetHashCode()];
                (_matrix[u, v] ??= new List<Edge>()).Add(e);
            }
            _dirty = false;
        }

        public Vertex? GetVertex(int id)
            => _byId.TryGetValue(id, out var v) ? v : null;

        public List<Edge> GetOutgoingEdges(int vertexId)
        {
            EnsureBuilt();
            var result = new List<Edge>();
            if (!_idToIndex.TryGetValue(vertexId, out int u)) return result;
            int n = _vertices.Count;
            for (int v = 0; v < n; v++)
                if (_matrix![u, v] != null) result.AddRange(_matrix[u, v]);
            return result;
        }

        public List<Edge> GetIncomingEdges(int vertexId)
        {
            EnsureBuilt();
            var result = new List<Edge>();
            if (!_idToIndex.TryGetValue(vertexId, out int v)) return result;
            int n = _vertices.Count;
            for (int u = 0; u < n; u++)
                if (_matrix![u, v] != null) result.AddRange(_matrix[u, v]);
            return result;
        }

        public IEnumerable<Vertex> GetAllVertices() => _vertices;
        public IEnumerable<Edge> GetAllEdges() => _edges;
        public bool ContainsVertex(int vertexId) => _byId.ContainsKey(vertexId);
    }
}
