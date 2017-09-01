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
    public class SummariserNode : Node
    {
        private double start;
        private double end;
        private Brush colour;
        public Brush Colour { get { return colour; } }

        public SummariserNode(double start, double end, string nodeName, Brush colour) : base(nodeName)
        {
            this.colour = colour;
            this.start = start;
            this.end = end;
        }
        public double[] Value()
        {
            return new double[] { start, end };
        }
    }
}
