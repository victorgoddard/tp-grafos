using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace graph_tp.Models
{
    public class Edge
    {
        private Vertex _source;
        private Vertex _target;
        private double _loadValue;
        private double _capacity;
        private bool _hasResidualCapacity;
        private double _flow;
        private double _residualCapacity;
        public double Low { get; set; }

        public Vertex Source => _source;
        public Vertex Target => _source;
        public double LoadValue => _loadValue;
        public double Capacity => _capacity;

        public Edge(Vertex source, Vertex target, double loadValue, double capacity = double.PositiveInfinity, double flow = 0)
        {
            _source = source;
            _target = target;
            _loadValue = loadValue;
            _capacity = capacity;
            _flow = flow;
            InitValues();
        }

        private void InitValues()
        {
            _hasResidualCapacity = HasResidualCapacity();
            CalculateResidualCapacity();
        }

        private bool HasResidualCapacity()
        {
            bool isValid = false;

            if (_capacity != double.PositiveInfinity && _flow != 0)
            { isValid = true; CalculateResidualCapacity(); }

            return isValid;
        }

        private double CalculateResidualCapacity()
        {
            double residualCapacity = 0;
            if (_hasResidualCapacity)
                residualCapacity = _capacity - _flow;

            return residualCapacity;
            //_residualCapacity cant be negative Exception
        }

        public override string ToString()
        {
            String toString = string.Empty;

            if (_hasResidualCapacity)
                toString = "Testing...";

            return toString;
        }

        public Edge Clone()
        {
            return new Edge(_source, _target, _loadValue, _capacity, _flow);
        }
    }
}
