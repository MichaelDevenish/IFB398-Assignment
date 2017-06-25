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
        Button nodeButton;
        int x;
        int y;
        public GraphNode(int x, int y, Brush brush)
        {
            nodeButton = new Button();
            nodeButton.Width = 10;
            nodeButton.Height = 10;
            nodeButton.Background = brush;
            this.x = x;
            this.y = y;
        }

        public void AddButtonClick(RoutedEventHandler click)
        {
            nodeButton.Click += click;
        }
        public void AddButtonHover(MouseEventHandler hover)
        {
            nodeButton.MouseEnter += hover;
        }
        public int[] GetCoords()
        {
            return new int[] { x, y };
        }
        
    }
}
