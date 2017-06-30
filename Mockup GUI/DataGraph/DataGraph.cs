using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DataGraph
{

    public class TestGraph : Canvas
    {
        //constants
        const double XMIN = 80;
        private const int NAME_HORIZONTAL_OFFSET = 15;
        private const int TEXT_HEIGHT = 20;
        private const int YMIN_DEFAULT = 115;
        private const int YMIN_NO_SUMMARISER = 30;
        private const int YMIN_INCREMENTSIZE = 20;
        private const int NUMBEROFYMININCREMENTS = 4;
        private const int KEY_ICONSIZE = 20;
        private const int KEY_GAP = 3;
        private const int BUTTON_SIZE = 10;
        private const int GRAPH_LINE_SIZE = 3;
        private const int DEFAULT_RECTANGLE_BORDER = 0;
        private const int X_LINE_MARKE_RSIZE = 5;
        private const int X_LINE_THICKNESS = 1;
        private const double Y_LINE_THICKNESS = 0.5;
        private const int Y_NUMBER_HORIZONTAL_OFFSET = 8;
        private const int Y_NUMBER_VERTICAL_OFFSET = 8;
        private const int SUMMARISER_TEXT_OFFSET = 5;
        private const int X_NUMBERHORIZONTAL_OFFSET = 8;
        private const int X_NUMBER_HORIZONTAL_UNDER = 4;

        //globals
        private static Brush[] otherBrushes = new Brush[]{ Brushes.MediumTurquoise, Brushes.LightGreen, Brushes.Gray, Brushes.Salmon,
             Brushes.Orange, Brushes.Crimson, Brushes.Black, Brushes.SteelBlue, Brushes.Orange, Brushes.Tan};
        private double ymin = YMIN_DEFAULT;
        private List<GraphDataset> datasets;
        private int xDivisor = 1;
        private int yDivisor = 1;
        private string xAxisName = "";
        private string yAxisName = "";
        private bool summariser = true;

        //properties
        public int XDivisor { get { return xDivisor; } set { xDivisor = value; } }
        public int YDivisor { get { return yDivisor; } set { yDivisor = value; } }
        public string XAxisName { set { xAxisName = value; } }
        public string YAxisName { set { yAxisName = value; } }

        //functions
        static TestGraph()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TestGraph), new FrameworkPropertyMetadata(typeof(TestGraph)));
        }

        /// <summary>
        /// Hides the lower part of the graph
        /// </summary>
        public void DisableSummariser()
        {
            summariser = false;
        }

        /// <summary>
        /// Adds the supplied dataset to the list of datasets
        /// </summary>
        /// <param name="data">the dataset to be added</param>
        public void AddDataset(GraphDataset data)
        {
            if (datasets == null) datasets = new List<GraphDataset>();
            if (!datasets.Contains(data) && datasets.Count < 5) datasets.Add(data);
        }

        /// <summary>
        /// Draws the graph and the stored datasets
        /// </summary>
        public void DrawGraph()
        {
            double xdatamax = 0;
            double ydatamax = 0;
            Dictionary<string, int> nodeNames = new Dictionary<string, int>(); //add occurrences to get priority

            if (datasets != null)
            {
                foreach (GraphDataset data in datasets)
                {
                    foreach (GraphNode node in data.Nodes)
                    {
                        double[] XY = node.GetCoords();
                        if (XY[0] > xdatamax) xdatamax = XY[0];
                        if (XY[1] > ydatamax) ydatamax = XY[1];

                        if (!nodeNames.ContainsKey(node.NodeName)) nodeNames.Add(node.NodeName, 1);
                        else nodeNames[node.NodeName] = nodeNames[node.NodeName] + 1;
                    }

                }
                xdatamax = Math.Ceiling(xdatamax / xDivisor) * xDivisor;
                ydatamax = Math.Ceiling(ydatamax / yDivisor) * yDivisor;
                SetYmin(nodeNames);
                if (ymin != YMIN_NO_SUMMARISER)
                {
                    List<string> topData = DrawSummariserLayout(nodeNames, xdatamax);
                    DrawSummariserData(topData, datasets, xdatamax, nodeNames.Count);
                }
            }

            DrawXAxis(xdatamax);
            DrawYAxis(ydatamax);

            if (xAxisName != "") Children.Add(GenerateGraphText(xAxisName, Width / 2, (Height - ymin + NAME_HORIZONTAL_OFFSET)));
            if (yAxisName != "") Children.Add(GenerateGraphText(yAxisName, 0, ((Height - ymin) / 2) - TEXT_HEIGHT / 2));
            if (datasets != null) DrawDatasets(xdatamax, ydatamax);
        }

        /// <summary>
        /// Sets the offset for the bottom of the graph depending on how many different 
        /// item names there are in the datasets and if the summarizer is disabled or not
        /// </summary>
        /// <param name="nodeNames">list of items names in the datasets</param>
        private void SetYmin(Dictionary<string, int> nodeNames)
        {
            ymin = YMIN_DEFAULT;
            if (summariser == false)
            {
                ymin = YMIN_NO_SUMMARISER;
                return;
            }
            if (nodeNames.Count < 4)
            {
                ymin -= YMIN_INCREMENTSIZE * (NUMBEROFYMININCREMENTS - nodeNames.Count());
            }
        }

        /// <summary>
        /// Draws the general layout of the summarizer based on what nodeNames exist and their occurrences 
        /// </summary>
        /// <param name="nodeNames">a dictionary that represents the node names that 
        /// exist in the datasets and how many times they occur</param>
        /// <returns>a list of the data that is not in the other section of the summarizer</returns>
        private List<string> DrawSummariserLayout(Dictionary<string, int> nodeNames, double xdatamax)
        {
            List<string> topData = new List<string>();
            int currentPosition = 0;
            int maxIteration = nodeNames.Count();

            GeometryGroup xaxis_geom = new GeometryGroup();
            if (nodeNames.Count() > 4)
            {
                maxIteration = 4;
                DrawSummariserRow(xaxis_geom, "Other", Height);
                currentPosition++;
            }
            for (int i = currentPosition; i < maxIteration; i++)
            {
                string maxName = GetNextMostCommonName(nodeNames, topData);
                double height = Height - (i * YMIN_INCREMENTSIZE);
                DrawSummariserRow(xaxis_geom, maxName, height);
                currentPosition++;
            }
            double topBorderHeight = Height - ((currentPosition) * YMIN_INCREMENTSIZE);
            xaxis_geom.Children.Add(new LineGeometry(new Point(0, topBorderHeight), new Point(Width, topBorderHeight)));
            xaxis_geom.Children.Add(new LineGeometry(new Point(0, topBorderHeight), new Point(0, Height)));
            for (int e = 0; e <= xdatamax; e++)
            {
                double xpos = XMIN + ((Width - XMIN) / xdatamax) * e;
                xaxis_geom.Children.Add(new LineGeometry(new Point(xpos, topBorderHeight), new Point(xpos, Height)));
            }
            Children.Add(GenerateSetOfLines(xaxis_geom, 1, Brushes.LightGray));
            return topData;

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
            Children.Add(GenerateGraphText(name, SUMMARISER_TEXT_OFFSET, height - TEXT_HEIGHT));
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
                    double xpoint1 = (smallestIncrement * dataset.Nodes[i].GetCoords()[0]) + XMIN;
                    double xpoint2 = i + 1 < dataset.Nodes.Count() ?
                        (smallestIncrement * dataset.Nodes[i + 1].GetCoords()[0]) + XMIN
                        : Width;
                    string itemName = dataset.Nodes[i].NodeName;
                    Rectangle rect = GenerateSummariserDatapoint(itemsNotInOther, orderOfOther, datasetHeight, e, itemName, dataset, xpoint1, xpoint2, countOfNames);
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
        /// <returns>a rectangle that represents a single data point in the summarizer</returns>
        private Rectangle GenerateSummariserDatapoint(List<string> notInOther, List<string> orderOfOther, double itemHeight, int verticalIndex, string itemName, GraphDataset parent, double start, double end, int countOfNames)
        {
            Rectangle rect = GenerateRectangle(itemHeight, end - start, parent.Colour, itemName);
            if (notInOther.Contains(itemName))
            {
                int startingPosition = TEXT_HEIGHT * 2;
                if (notInOther.Count() == NUMBEROFYMININCREMENTS) startingPosition = TEXT_HEIGHT;
                double v = Height - startingPosition - (TEXT_HEIGHT * notInOther.IndexOf(itemName)) + (itemHeight * verticalIndex);
                if (countOfNames < 4)
                    v += TEXT_HEIGHT;
                SetTop(rect, v);
            }
            else
            {
                if (!orderOfOther.Contains(itemName)) orderOfOther.Add(itemName);
                rect.Fill = otherBrushes[orderOfOther.IndexOf(itemName) % otherBrushes.Length];
                SetTop(rect, Height - TEXT_HEIGHT + (itemHeight * verticalIndex));
            }
            SetLeft(rect, start);
            return rect;
        }

        /// <summary>
        /// draws the datasets onto the graph
        /// </summary>
        /// <param name="xdatamax">the highest x value</param>
        /// <param name="ydatamax">the highest y value</param>
        private void DrawDatasets(double xdatamax, double ydatamax)
        {
            int keyOffset = 0;
            foreach (GraphDataset data in datasets)
            {
                keyOffset = AddDataToKey(keyOffset, data);
                DrawNodesForDataset(xdatamax, ydatamax, data);
                DrawButtonsForDataset(data);
            }
        }

        /// <summary>
        /// Draws the buttons onto the graph for the supplied dataset
        /// </summary>
        /// <param name="data">the dataset to draw the buttons for</param>
        private void DrawButtonsForDataset(GraphDataset data)
        {
            foreach (GraphNode node in data.Nodes)
            {
                node.SetButtonStyle(FindResource("CircleButton") as Style);
                Children.Add(node.NodeButton);
            }
        }

        /// <summary>
        /// draws the nodes onto the graph for the supplied dataset
        /// </summary>
        /// <param name="xdatamax">the largest x value</param>
        /// <param name="ydatamax">the largest y value</param>
        /// <param name="data">the dataset to draw the nodes for</param>
        private void DrawNodesForDataset(double xdatamax, double ydatamax, GraphDataset data)
        {
            PointCollection points = new PointCollection();
            foreach (GraphNode node in data.Nodes)
            {
                points.Add(SetupNodePosition(xdatamax, ydatamax, node));
            }
            Polyline polyline = GenerateGraphLine(data.Colour, points);
            Children.Add(polyline);
        }

        /// <summary>
        /// creates a Point on the graph using the supplied variables in a node converting the variables
        /// to ones relating to the x any y max points on the graph
        /// </summary>
        /// <param name="xdatamax">the maximum x value of the graph </param>
        /// <param name="ydatamax">the maximum y value of the graph</param>
        /// <param name="node">the node to be drawn on the graph</param>
        /// <returns>a point that represents the node</returns>
        private Point SetupNodePosition(double xdatamax, double ydatamax, GraphNode node)
        {
            double xpoint = (((Width - XMIN) / xdatamax) * node.GetCoords()[0]) + XMIN;
            double ypoint = (Height - ymin) - (((Height - ymin) / ydatamax) * node.GetCoords()[1]);
            SetLeft(node.NodeButton, xpoint - BUTTON_SIZE / 2);
            SetTop(node.NodeButton, ypoint - BUTTON_SIZE / 2);
            return new Point(xpoint, ypoint);
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

        /// <summary>
        /// adds a dataset to the graph key
        /// </summary>
        /// <param name="keyOffset">the vertical offset of the current key</param>
        /// <param name="data">the data to add</param>
        /// <returns>the offset of the next dataset if another was to be added</returns>
        private int AddDataToKey(int keyOffset, GraphDataset data)
        {
            Rectangle rect = GenerateRectangle(KEY_ICONSIZE, KEY_ICONSIZE, data.Colour, "");
            Label lab = new Label();
            lab.Content = data.DatasetName;

            SetTop(rect, Height - ymin - keyOffset);
            SetTop(lab, Height - ymin - keyOffset);
            SetLeft(lab, KEY_ICONSIZE);

            Children.Add(rect);
            Children.Add(lab);
            return keyOffset + KEY_ICONSIZE + KEY_GAP;
        }

        /// <summary>
        /// Draws the x axis of the graph
        /// </summary>
        /// <param name="xdatamax">the largest value in the x axis</param>
        private void DrawXAxis(double xdatamax)
        {
            GeometryGroup xaxis_geom = new GeometryGroup();
            xaxis_geom.Children.Add(new LineGeometry(new Point(XMIN, Height - ymin), new Point(Width, Height - ymin)));
            for (double x = 0; x <= xdatamax; x += xDivisor)
            {
                double point = (((Width - XMIN) / xdatamax) * (x)) + XMIN;
                double offset = getDoubleOffset(x, X_NUMBERHORIZONTAL_OFFSET) - X_NUMBER_HORIZONTAL_UNDER;
                xaxis_geom.Children.Add(new LineGeometry(new Point(point, Height - ymin - X_LINE_MARKE_RSIZE), new Point(point, Height - ymin)));
                Children.Add(GenerateGraphText(x.ToString(), point - offset, (Height - ymin)));
            }
            Children.Add(GenerateSetOfLines(xaxis_geom, X_LINE_THICKNESS, Brushes.Black));
        }

        /// <summary>
        /// Draws the y axis of the graph
        /// </summary>
        /// <param name="ydatamax">the largest value in the y axis</param>
        private void DrawYAxis(double ydatamax)
        {
            GeometryGroup yaxis_geom = new GeometryGroup();
            for (double y = 0; y <= ydatamax; y += yDivisor)
            {
                if (y != 0)
                {
                    double point = (Height - ymin) - (((Height - ymin) / ydatamax) * (y));
                    double offset = getDoubleOffset(y, Y_NUMBER_HORIZONTAL_OFFSET);
                    yaxis_geom.Children.Add(new LineGeometry(new Point(Width, point), new Point(XMIN, point)));
                    Children.Add(GenerateGraphText(y.ToString(), XMIN - offset, (point - Y_NUMBER_VERTICAL_OFFSET)));
                }
            }
            Children.Add(GenerateSetOfLines(yaxis_geom, Y_LINE_THICKNESS, Brushes.Gray));
        }

        /// <summary>
        /// Gets the horizontal offset of a double depending on how many positive numbers it has in it
        /// </summary>
        /// <param name="value">the value to offset</param>
        /// <param name="offsetIncrease">the increment size of the offset</param>
        /// <returns>the desired offset</returns>
        private double getDoubleOffset(double value, int offsetIncrease)
        {
            return value == 0 ? offsetIncrease : offsetIncrease * Math.Floor(Math.Log10(Math.Abs(value)) + 1);
        }

        /// <summary>
        /// Generates a TextBlock from the supplied string at the supplied positions
        /// </summary>
        /// <param name="text">the text to be written into the TextBlock</param>
        /// <param name="x">the x position of the TextBlock</param>
        /// <param name="y">the y position of the TextBlock</param>
        /// <returns></returns>
        private TextBlock GenerateGraphText(string text, double x, double y)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.Foreground = Brushes.Black;
            SetLeft(textBlock, x);
            SetTop(textBlock, y);
            return textBlock;
        }

        /// <summary>
        /// Generates a set of lines in a geometry group
        /// </summary>
        /// <param name="lines">a GeometryGroup that represents the lines</param>
        /// <param name="thickness">the desired thickness</param>
        /// <param name="brush">the desired colour</param>
        /// <returns>a path that has been built from the GeometryGroup</returns>
        private Path GenerateSetOfLines(GeometryGroup lines, double thickness, SolidColorBrush brush)
        {
            Path axis_path = new Path();
            axis_path.StrokeThickness = thickness;
            axis_path.Stroke = brush;
            axis_path.Data = lines;
            return axis_path;
        }

        /// <summary>
        /// Generates a rectangle with the supplied dimensions, colour and tool-tip (if a blank string is supplied no tool-tip is added)
        /// </summary>
        /// <param name="height">the height of the rectangle</param>
        /// <param name="width">the width of the rectangle</param>
        /// <param name="colour">the desired colour</param>
        /// <param name="tooltip">the desired tool-tip</param>
        /// <returns></returns>
        private static Rectangle GenerateRectangle(double height, double width, Brush colour, string tooltip)
        {
            Rectangle rect = new Rectangle();
            rect.Height = height;
            rect.Width = width;
            rect.Fill = colour;
            rect.StrokeThickness = DEFAULT_RECTANGLE_BORDER;
            if (tooltip != "") rect.ToolTip = tooltip;
            return rect;
        }

        /// <summary>
        /// generates a line of the supplied colour between the supplied points
        /// </summary>
        /// <param name="brush">the desired colour</param>
        /// <param name="points">the points to draw the line between</param>
        /// <returns>the generated line</returns>
        private Polyline GenerateGraphLine(Brush brush, PointCollection points)
        {
            Polyline polyline = new Polyline();
            polyline.StrokeThickness = GRAPH_LINE_SIZE;
            polyline.Stroke = brush;
            polyline.Points = points;
            return polyline;

        }

    }
}
