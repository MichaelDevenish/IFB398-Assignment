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
        private double x;
        private double y;

        public string NodeName { get { return nodeName; } }

        public GraphNode(double x, double y, string activityName)
        {
            this.nodeName = activityName;
            nodeButton = new Button();
            nodeButton.Width = 10;
            nodeButton.Height = 10;
            this.x = x;
            this.y = y;
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
            return new double[] { x, y };
        }

    }
}
