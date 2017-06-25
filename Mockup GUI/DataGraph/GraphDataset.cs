using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGraph
{
    public class GraphDataset
    {
        private List<GraphNode> nodes;
        public List<GraphNode> Nodes { get { return nodes; } }
        public GraphDataset()
        {
            nodes = new List<GraphNode>();
        }
        public void AddNode(GraphNode node)
        {
            nodes.Add(node);
        }
    }
}
