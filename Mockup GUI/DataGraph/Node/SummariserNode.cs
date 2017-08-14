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

        public SummariserNode(double start, double end, string nodeName) : base(nodeName)
        {
            this.start = start;
            this.end = end;
        }
        public double[] Value()
        {
            return new double[] { start, end };
        }
    }
}
