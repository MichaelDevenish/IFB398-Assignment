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
        private List<Node> nodes;
        private string datasetName;
        private Brush colour;

        public List<Node> Nodes { get { return nodes; } }
        public string DatasetName { get { return datasetName; } }
        public Brush Colour { get { return colour; } }

        public GraphDataset(string name)
        {
            datasetName = name;
            nodes = new List<Node>();
        }
        public void AddNode(Node node)
        {
            nodes.Add(node);
        }
    }
}
