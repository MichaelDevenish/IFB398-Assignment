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
    public class GraphNode
    {
        private Button nodeButton;
        private string nodeName;
        private double horizontalValue;
        private double verticleValue;

        public string NodeName { get { return nodeName; } }
        public Button NodeButton { get { return nodeButton; } }

        public GraphNode(double horizontalValue, double verticleValue, string nodeName)
        {
            this.nodeName = nodeName;
            nodeButton = new Button();
            var myResourceDictionary = new ResourceDictionary
            {
                Source = new Uri("/DataGraph;component/Dictionary.xaml", UriKind.RelativeOrAbsolute)
            };
            nodeButton.Style = myResourceDictionary["CircleButton"] as Style;
            nodeButton.Width = 10;
            nodeButton.Height = 10;
            this.horizontalValue = horizontalValue;
            this.verticleValue = verticleValue;
        }

        public void SetButtonColour(Brush brush)
        {
            nodeButton.Background = brush;
        }

        public void AddButtonClick(RoutedEventHandler click)
        {
            nodeButton.Click += click;
        }
        public void AddButtonHover(MouseEventHandler hover)
        {
            nodeButton.MouseEnter += hover;
        }
        public double[] GetCoords()
        {
            return new double[] { horizontalValue, verticleValue };
        }

    }
}
