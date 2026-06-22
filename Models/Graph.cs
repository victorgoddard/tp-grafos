using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace graph_tp.Models
{
    public class Graph
    {
        private readonly Dictionary<int, Vertex> _vertexes;
        private readonly Dictionary<int, List<Edge>> _adjacencyList;

        public int VertexCount => _vertexes.Count();
        public int EdgesCount => _adjacencyList.Values.Sum(edges => edges.Count);

        public Graph()
        {
            _vertexes = new Dictionary<int, Vertex>();
            _adjacencyList = new Dictionary<int, List<Edge>>();
        }

        public void AddVertex(Vertex vertex)
        {
            if (!_vertexes.ContainsKey(vertex.GetHashCode()))
            {
                _vertexes[vertex.GetHashCode()] = vertex;
                _adjacencyList[vertex.GetHashCode()] = new List<Edge>();
            }
        }

        public void AddEdge(Edge edge)
        {
            AddVertex(edge.Source);
            AddVertex(edge.Target);

            _adjacencyList[edge.Source.GetHashCode()].Add(edge);
        }

        public Vertex? GetVertex(int id)
        {
            return _vertexes.TryGetValue(id, out var vertex) ? vertex : null;
        }

        public List<Edge> GetOutgoingEdges(int vertexId)
        {
            return _adjacencyList.TryGetValue(vertexId, out var edges) ? edges : new List<Edge>();
        }

        public List<Edge> GetIncomingEdges(int vertexId)
        {
            var incoming = new List<Edge>();
            foreach (var edges in _adjacencyList.Values)
            {
                incoming.AddRange(edges.Where(e => e.Target.GetHashCode() == vertexId));
            }
            return incoming;
        }

        public IEnumerable<Vertex> GetAllvertexs()
        {
            return _vertexes.Values;
        }

        public IEnumerable<Edge> GetAllEdges()
        {
            return _adjacencyList.Values.SelectMany(edges => edges);
        }

        public bool Containsvertex(int vertexId)
        {
            return _vertexes.ContainsKey(vertexId);
        }

        public int GetOutDegree(int vertexId)
        {
            return GetOutgoingEdges(vertexId).Count;
        }

        public int GetInDegree(int vertexId)
        {
            return GetIncomingEdges(vertexId).Count;
        }

        public Graph Clone()
        {
            var clone = new Graph();

            foreach (var vertex in _vertexes.Values)
            {
                clone.AddVertex(new Vertex(vertex.GetHashCode(), vertex.Name));
            }

            foreach (var edge in GetAllEdges())
            {
                var sourceClone = clone.GetVertex(edge.Source.GetHashCode())!;
                var targetClone = clone.GetVertex(edge.Target.GetHashCode())!;
                clone.AddEdge(new Edge(sourceClone, targetClone, edge.LoadValue, edge.Capacity));
            }

            return clone;
        }
    }
}