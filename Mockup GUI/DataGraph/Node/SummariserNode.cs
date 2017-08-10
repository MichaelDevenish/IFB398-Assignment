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
        private double value;
        public double Value { get { return value; } }
        public SummariserNode(double value, string nodeName) : base(nodeName)
        {
            this.value = value;
        }
    }
}
