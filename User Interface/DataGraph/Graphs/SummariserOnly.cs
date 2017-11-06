using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DataGraph
{
    public class SummariserOnly : Graph
    {

        //TODO CLEANUP AND REMOVE UNUSED JUNK
        const double XMIN = 105;
        private const int TEXT_HEIGHT = 20;
        private const int YMIN_INCREMENTSIZE = 20;
        private const int NUMBEROFYMININCREMENTS = 25;
        private const int SUMMARISER_TEXT_OFFSET = 5;

        static SummariserOnly()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SummariserOnly), new FrameworkPropertyMetadata(typeof(SummariserOnly)));
        }

        public List<GraphDataset> datasets;

        public int NumOfDatasets { get { return datasets.Count; } }
        private int divisor = 1;

        /// <summary>
        /// Adds the supplied dataset to the list of datasets
        /// </summary>
        /// <param name="data">the dataset to be added</param>
        /// <returns>true if dataset is successfully added</returns>
        public bool AddDataset(GraphDataset data)
        {
            if (datasets == null) datasets = new List<GraphDataset>();
            if (!datasets.Contains(data) && datasets.Count < 5)
            {
                datasets.Add(data);
                return true;
            }
            return false;
        }

        public void ClearDatasets()
        {
            datasets.Clear();
        }

        public int Divisor { get { return divisor; } set { divisor = value; } }
        /// <summary>
        /// Draws the graph and the stored datasets
        /// </summary>
        public void DrawGraph(double length)
        {
            Children.Clear();
            if (double.IsNaN(Width)) throw new NotFiniteNumberException("Graph must have a set width!", Width);
            if (double.IsNaN(Height)) throw new NotFiniteNumberException("Graph must have a set height!", Height);
            if (datasets == null || datasets.Count() == 0) throw new NoDatasetsException("Must have at least added one dataset to draw a graph!");
            double datamax = 0;
            Dictionary<string, int> nodeNames = new Dictionary<string, int>(); //add occurrences to get priority

            if (datasets != null)
            {
                foreach (GraphDataset data in datasets)
                {
                    foreach (SummariserNode node in data.Nodes)
                    {
                        datamax = (node.Value()[1] > datamax) ? node.Value()[1] : datamax;
                        if (!nodeNames.ContainsKey(node.NodeName)) nodeNames.Add(node.NodeName, 1);
                        else nodeNames[node.NodeName] = nodeNames[node.NodeName] + 1;
                    }

                }
                Height = (nodeNames.Count < NUMBEROFYMININCREMENTS) ? nodeNames.Count * YMIN_INCREMENTSIZE : NUMBEROFYMININCREMENTS * YMIN_INCREMENTSIZE;
                //Children.Add(GenerateGraphText(Height.ToString(), SUMMARISER_TEXT_OFFSET, 0));
                //datamax = (Math.Ceiling(datamax / divisor) * divisor);
                //datamax = length; //CHANGE THIS TO TRUE WHEN USING ACTUAL VIDEOS TO MAKE ACCURATE todo
                List<string> topData = new List<string>();
                Path path = DrawSummariserLayout(nodeNames, datamax, topData);
                DrawSummariserData(topData, datasets, datamax, nodeNames.Count);
                Children.Add(path);
            }
        }



        /// <summary>
        /// Draws the general layout of the summarizer based on what nodeNames exist and their occurrences 
        /// </summary>
        /// <param name="nodeNames">a dictionary that represents the node names that 
        /// exist in the datasets and how many times they occur</param>
        /// <returns>a list of the data that is not in the other section of the summarizer</returns>
        private Path DrawSummariserLayout(Dictionary<string, int> nodeNames, double xdatamax, List<string> topData)
        {
            int currentPosition = 0;
            int maxIteration = nodeNames.Count;

            GeometryGroup xaxis_geom = new GeometryGroup();
            if (maxIteration > NUMBEROFYMININCREMENTS)
            {
                maxIteration = NUMBEROFYMININCREMENTS;
                DrawSummariserRow(xaxis_geom, "Other", (NUMBEROFYMININCREMENTS - 1) * YMIN_INCREMENTSIZE);
                currentPosition++;
            }
            for (int i = 0; currentPosition < maxIteration; i++)
            {
                string maxName = GetNextMostCommonName(nodeNames, topData);
                double height = i * YMIN_INCREMENTSIZE;
                DrawSummariserRow(xaxis_geom, maxName, height);
                currentPosition++;
            }

            double topBorderHeight = ((currentPosition) * YMIN_INCREMENTSIZE);
            xaxis_geom.Children.Add(new LineGeometry(new Point(0, topBorderHeight), new Point(Width, topBorderHeight)));
            xaxis_geom.Children.Add(new LineGeometry(new Point(0, 0), new Point(0, Height)));
            xaxis_geom.Children.Add(new LineGeometry(new Point(XMIN, 0), new Point(XMIN, Height)));
            xaxis_geom.Children.Add(new LineGeometry(new Point(Width, 0), new Point(Width, Height)));
            //for (int e = 0; e <= xdatamax; e++)
            //{
            //    double xpos = XMIN + ((Width - XMIN) / xdatamax) * e;
            //    xaxis_geom.Children.Add(new LineGeometry(new Point(xpos, topBorderHeight), new Point(xpos, 0)));
            //}
            return GenerateSetOfLines(xaxis_geom, 1, Brushes.LightGray);

        }

        /// <summary>
        /// Draws a single row in the summarizer 
        /// </summary>
        /// <param name="xaxis_geom">the object to add the line to</param>
        /// <param name="name">the name of the row</param>
        /// <param name="height">the vertical position of the row</param>
        private void DrawSummariserRow(GeometryGroup xaxis_geom, string name, double height)
        {
            xaxis_geom.Children.Add(new LineGeometry(new Point(0, height), new Point(Width, height)));
            Children.Add(GenerateGraphText(name, SUMMARISER_TEXT_OFFSET, height));
        }

        /// <summary>
        /// Draws the data points in the summarizer
        /// </summary>
        /// <param name="itemsNotInOther">the item names of the items that are not in the other section</param>
        /// <param name="datasets">the datasets to be added</param>
        /// <param name="xdatamax">the largest horizontal number on the graph</param>
        private void DrawSummariserData(List<string> itemsNotInOther, List<GraphDataset> datasets, double xdatamax, int countOfNames)
        {
            List<string> orderOfOther = new List<string>();
            double datasetHeight = YMIN_INCREMENTSIZE / datasets.Count();
            double smallestIncrement = (Width - XMIN) / xdatamax;

            for (int e = 0; e < datasets.Count(); e++)
            {
                GraphDataset dataset = datasets[e];
                for (int i = 0; i < dataset.Nodes.Count(); i++)
                {
                    double[] value = ((SummariserNode)dataset.Nodes[i]).Value();
                    double xpoint1 = smallestIncrement * value[0] + XMIN;
                    double xpoint2 = smallestIncrement * value[1] + XMIN;
                    string itemName = dataset.Nodes[i].NodeName;
                    Rectangle rect = GenerateSummariserDatapoint(itemsNotInOther, orderOfOther, datasetHeight, e, itemName, ((SummariserNode)dataset.Nodes[i]).Colour, xpoint1, xpoint2, countOfNames);
                    Children.Add(rect);
                }
            }

        }

        /// <summary>
        /// Creates a rectangle that represents a single data point in the summarizer
        /// </summary>
        /// <param name="notInOther">the names of the data points that are not in the other classification</param>
        /// <param name="orderOfOther">the order of appearance of items in other, used to choose colour</param>
        /// <param name="itemHeight">the height of the data point</param>
        /// <param name="verticalIndex">the vertical index inside of the row of the data point</param>
        /// <param name="itemName">the name of the item</param>
        /// <param name="parent">the parent dataset of the item</param>
        /// <param name="start">the starting x position of the data point</param>
        /// <param name="end">the ending x position of the data point</param>
        /// <param name="countOfNames">the number of names that are being used for points</param>
        /// <returns>a rectangle that represents a single data point in the summarizer</returns>
        private Rectangle GenerateSummariserDatapoint(List<string> notInOther, List<string> orderOfOther, double itemHeight, int verticalIndex, string itemName, Brush colour, double start, double end, int countOfNames)
        {
            Rectangle rect = GenerateRectangle(itemHeight, end - start, colour, itemName);
            if (notInOther.Contains(itemName))
            {
                int startingPosition = 0;
                double v = startingPosition + (TEXT_HEIGHT * notInOther.IndexOf(itemName)) + (itemHeight * verticalIndex);
                SetTop(rect, v);
            }
            else
            {
                if (!orderOfOther.Contains(itemName)) orderOfOther.Add(itemName);
                rect.Fill = otherBrushes[orderOfOther.IndexOf(itemName) % otherBrushes.Length];
                SetTop(rect, (NUMBEROFYMININCREMENTS - 1) * YMIN_INCREMENTSIZE);
            }
            SetLeft(rect, start);
            return rect;
        }

        /// <summary>
        /// Gets the most common name in nodeNames that is not already in alreadyUsedNames, adds it to alreadyUsedNames and returns it
        /// </summary>
        /// <param name="nodeNames">the Dictionary to search with the key being the name and the variable being the occurrences</param>
        /// <param name="alreadyUsedNames">names that have already been used</param>
        /// <returns></returns>
        private string GetNextMostCommonName(Dictionary<string, int> nodeNames, List<string> alreadyUsedNames)
        {
            string maxName = "";
            int max = 0;
            foreach (KeyValuePair<string, int> item in nodeNames)
            {
                string key = item.Key;
                int val = item.Value;
                if (!alreadyUsedNames.Contains(key) && val > max)
                {
                    max = val;
                    maxName = key;
                }
            }
            alreadyUsedNames.Add(maxName);
            return maxName;
        }

    }
}
