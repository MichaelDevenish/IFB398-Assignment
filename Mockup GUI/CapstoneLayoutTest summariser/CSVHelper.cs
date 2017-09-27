using DataGraph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CapstoneLayoutTest
{
    public  class CSVDatasetLoader
    {
        List<string[]> lines;
        public CSVDatasetLoader(string url)
        {
            lines = File.ReadAllLines(url).Select(a => a.Split(',')).ToList();
        }

        public List<double> AppendSortedStartList(List<double> start)
        {
            start = start.Concat(lines.Skip(1).ToList().Select(a => double.Parse(a.ElementAt(1)))).ToList();
            start.Sort();
            return start;
        }

        public List<double> AppendSortedEndList(List<double> end)
        {
            end = end.Concat(lines.Skip(1).ToList().Select(a => double.Parse(a.ElementAt(2)))).ToList();
            end.Sort();
            return end;
        }

        public GraphDataset GenerateDataset(string name)
        {
            GraphDataset temp = new GraphDataset(name);
            foreach (string[] line in lines)
            {
                try
                {
                    double per = double.Parse(line[0]) * 100;
                    Brush col = PercentToProbabilityColour(per);
                    SummariserNode node = new SummariserNode(double.Parse(line[1]), double.Parse(line[2]), line[3], col);
                    temp.AddNode(node);
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return temp;
        }
        private static Brush PercentToProbabilityColour(double percent)
        {
            byte red = (byte)((percent > 51) ? 255 * (1 - 2 * (percent - 50)) / 100 : 255);
            byte green = (byte)((percent > 50) ? 255 : 255 * (2 * percent / 100));
            byte blue = 0;
            Brush color = new SolidColorBrush(Color.FromRgb(red, green, blue));
            return color;
        }
    }
}
