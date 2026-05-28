using UnityEngine;

namespace GraphModel
{
    public class Edge
    {
        public Node NodeA { get; set; }
        public Node NodeB { get; set; }

        public float DirRadianFromA { get; set; }
        public float DirRadianFromB { get; set; }


        public Vector2 leftTop = Vector3.zero;
        public Vector2 rightbot = Vector3.zero;

        public Edge(Node first, Node second)
        {
            NodeA = first;
            NodeB = second;

            DirRadianFromA = Mathf.Atan2(second.Y - first.Y, second.X - first.X);
            DirRadianFromB = Mathf.Atan2(first.Y - second.Y, first.X - second.X);

            first.AddEdge(this);
            second.AddEdge(this);
        }

    }
}
