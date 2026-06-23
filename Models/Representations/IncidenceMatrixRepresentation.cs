using System.Collections.Generic;
using graph_tp.Models;

namespace graph_tp.Models.Representations
{
    public class IncidenceMatrixRepresentation : IGraphRepresentation
    {
        private readonly List<Vertex> _vertices = new();
        private readonly Dictionary<int, Vertex> _byId = new();
        private readonly List<Edge> _edges = new();

        private Dictionary<int, int> _idToIndex = new();
        private int[,]? _matrix; // linhas = vértices, colunas = arestas
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
            int m = _edges.Count;
            _idToIndex = new Dictionary<int, int>(n);
            for (int i = 0; i < n; i++)
                _idToIndex[_vertices[i].GetHashCode()] = i;

            _matrix = new int[n, m];
            for (int c = 0; c < m; c++)
            {
                var e = _edges[c];
                int s = _idToIndex[e.Source.GetHashCode()];
                int t = _idToIndex[e.Target.GetHashCode()];
                if (s == t)
                {
                    _matrix[s, c] = 2; // laço
                }
                else
                {
                    _matrix[s, c] = -1;
                    _matrix[t, c] = 1;
                }
            }
            _dirty = false;
        }

        public Vertex? GetVertex(int id)
            => _byId.TryGetValue(id, out var v) ? v : null;

        public List<Edge> GetOutgoingEdges(int vertexId)
        {
            EnsureBuilt();
            var result = new List<Edge>();
            if (!_idToIndex.TryGetValue(vertexId, out int r)) return result;
            int m = _edges.Count;
            for (int c = 0; c < m; c++)
                if (_matrix![r, c] == -1 || _matrix[r, c] == 2) result.Add(_edges[c]);
            return result;
        }

        public List<Edge> GetIncomingEdges(int vertexId)
        {
            EnsureBuilt();
            var result = new List<Edge>();
            if (!_idToIndex.TryGetValue(vertexId, out int r)) return result;
            int m = _edges.Count;
            for (int c = 0; c < m; c++)
                if (_matrix![r, c] == 1 || _matrix[r, c] == 2) result.Add(_edges[c]);
            return result;
        }

        public IEnumerable<Vertex> GetAllVertices() => _vertices;
        public IEnumerable<Edge> GetAllEdges() => _edges;
        public bool ContainsVertex(int vertexId) => _byId.ContainsKey(vertexId);
    }
}
