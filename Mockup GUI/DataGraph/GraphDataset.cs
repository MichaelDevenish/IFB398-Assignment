using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DataGraph
{
    public class GraphDataset
    {
        private List<GraphNode> nodes;
        private string datasetName;
        private Brush colour;

        public List<GraphNode> Nodes { get { return nodes; } }
        public string DatasetName { get { return datasetName; } }
        public Brush Colour { get { return colour; } }

        public GraphDataset(string name, Brush colour)
        {
            this.colour = colour;
            datasetName = name;
            nodes = new List<GraphNode>();
        }
        public void AddNode(GraphNode node)
        {
            node.SetButtonColour(colour);
            nodes.Add(node);
        }
    }
}
