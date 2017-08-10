using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DataGraph
{
    public class GraphNode : Node
    {
        private double horizontalValue;
        private double verticleValue;


        public GraphNode(double horizontalValue, double verticleValue, string nodeName) : base(nodeName)
        {
            var myResourceDictionary = new ResourceDictionary
            {
                Source = new Uri("/DataGraph;component/Dictionary.xaml", UriKind.RelativeOrAbsolute)
            };
            this.horizontalValue = horizontalValue;
            this.verticleValue = verticleValue;
        }

       
        public double[] GetCoords()
        {
            return new double[] { horizontalValue, verticleValue };
        }

    }
}
