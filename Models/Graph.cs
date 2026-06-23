using System.Collections.Generic;
using graph_tp.Models.Representations;

namespace graph_tp.Models
{
    public class Graph
    {
        private IGraphRepresentation _representation;
        private RepresentationKind _currentKind;

        public int VertexCount => _representation.VertexCount;
        public int EdgesCount => _representation.EdgeCount;
        public RepresentationKind CurrentRepresentation => _currentKind;

        public Graph()
        {
            _currentKind = RepresentationKind.AdjacencyList;
            _representation = GraphRepresentationSelector.Create(_currentKind);
        }

        public void AddVertex(Vertex vertex) => _representation.AddVertex(vertex);

        public void AddEdge(Edge edge) => _representation.AddEdge(edge);

        public Vertex? GetVertex(int id) => _representation.GetVertex(id);

        public List<Edge> GetOutgoingEdges(int vertexId) => _representation.GetOutgoingEdges(vertexId);

        public List<Edge> GetIncomingEdges(int vertexId) => _representation.GetIncomingEdges(vertexId);

        public IEnumerable<Vertex> GetAllvertexs() => _representation.GetAllVertices();

        public IEnumerable<Edge> GetAllEdges() => _representation.GetAllEdges();

        public bool Containsvertex(int vertexId) => _representation.ContainsVertex(vertexId);

        public int GetOutDegree(int vertexId) => _representation.GetOutgoingEdges(vertexId).Count;

        public int GetInDegree(int vertexId) => _representation.GetIncomingEdges(vertexId).Count;

        // Mede a densidade e migra para a melhor estrutura, se necessário.
        public void OptimizeRepresentation()
        {
            var best = GraphRepresentationSelector.Select(VertexCount, EdgesCount);
            if (best == _currentKind) return;

            var next = GraphRepresentationSelector.Create(best);
            foreach (var vertex in _representation.GetAllVertices())
                next.AddVertex(vertex);
            foreach (var edge in _representation.GetAllEdges())
                next.AddEdge(edge);

            _representation = next;
            _currentKind = best;
        }

        public string GetRepresentationReport()
            => GraphRepresentationSelector.Report(VertexCount, EdgesCount, _currentKind);

        public Graph Clone()
        {
            var clone = new Graph();

            foreach (var vertex in _representation.GetAllVertices())
                clone.AddVertex(new Vertex(vertex.GetHashCode(), vertex.Name));

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
