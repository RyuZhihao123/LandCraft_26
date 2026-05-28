using System.Collections.Generic;

namespace GraphModel
{
    public class Graph
    {
        public List<Node> MajorNodes { get; set; } //For the major roads
        public List<Edge> MajorEdges { get; set; }
        public List<Node> MinorNodes { get; set; } //For the minor roads
        public List<Edge> MinorEdges { get; set; }




        public Graph()
        {
            MajorNodes = new List<Node>();
            MinorNodes = new List<Node>();

            MajorEdges = new List<Edge>();
            MinorEdges = new List<Edge>();
        }
    }
}
