using DataGraph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CapstoneLayoutTest.Helper_Functions
{
    /// <summary>
    /// Used to load in a compressed zip file to show on the GUI
    /// </summary>
    public class CSVDatasetLoader
    {
        List<string[]> lines;
        public CSVDatasetLoader(string url)
        {
            lines = File.ReadAllLines(url).Select(a => a.Split(',')).ToList();
        }

        /// <summary>
        /// Adds the data at the supplied position in the list string array lines to a supplied list and then sorts the list
        /// </summary>
        /// <param name="position">the string position in the string array lines</param>
        /// <param name="ListToAddTo">the sorted list to add the data to</param>
        /// <returns>a sorted ListToAddTo list with the added data</returns>
        public List<double> AppendDataToSortedList(int position, List<double> ListToAddTo)
        {
            ListToAddTo = ListToAddTo.Concat(lines.Skip(1).ToList().Select(a => double.Parse(a.ElementAt(position)))).ToList();
            ListToAddTo.Sort();
            return ListToAddTo;
        }

        /// <summary>
        /// Generates a GraphDataset using SummariserNode points from the data stored in lines
        /// </summary>
        /// <param name="name">the name of the GraphDataset</param>
        /// <returns>the completed dataset</returns>
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

        /// <summary>
        /// creates a brush on a hue range between 0 for red, 50 for yellow and 100 for green
        /// </summary>
        /// <param name="percent">the percent between 0 and 100 that represents the colour</param>
        /// <returns>the desired brush</returns>
        private static Brush PercentToProbabilityColour(double percent)
        {
            double t = ((2 * (percent - 50)) / 100);
            byte red = (byte)((percent > 51) ? 255 - (255 * ((2 * (percent - 50)) / 100)) : 255);
            byte green = (byte)((percent > 50) ? 255 : 255 * (2 * percent / 100));
            byte blue = 0;
            Brush color = new SolidColorBrush(Color.FromRgb(red, green, blue));
            return color;
        }
    }
}
