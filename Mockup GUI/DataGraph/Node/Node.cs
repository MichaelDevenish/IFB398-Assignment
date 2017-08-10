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
    public class Node
    {
        private string nodeName;
        public string NodeName { get { return nodeName; } }

        public Node(string nodeName)
        {
            this.nodeName = nodeName;
        }
    }
}
