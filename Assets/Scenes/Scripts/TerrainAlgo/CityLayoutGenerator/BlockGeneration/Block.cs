using System.Collections.Generic;

namespace BlockGeneration
{
    public class Block
    {
        public List<BlockNode> Nodes { get; set; }
        public float Height { get; set; }
        public bool IsPark { get; set; }

        public Block()
        {
            Nodes = new List<BlockNode>();
            Height = 0;
            IsPark = false;
        }
    }
}
