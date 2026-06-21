using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace graph_tp.Models
{
    internal class Vertex
    {

        private string _title;
        private int _id;

        public Vertex(int id, string title = "")
        {
            _id = id;
            _title = String.IsNullOrEmpty(title) ? SetName() : title;
        }

        private string SetName()
        {
            return $"Hub_{_id}";
        }

        public override string ToString()
        {
            return $"Vertex {_id} ({_title})";
        }

        public override bool Equals(object? obj)
        {
            return obj is Vertex vertex && obj.GetHashCode() == _id;
        }

        public override int GetHashCode()
        {
            return _id;
        }
    }
}
