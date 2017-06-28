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
        private const int KEY_ICON_BORDER = 0;
        private const int X_LINE_MARKE_RSIZE = 5;
        private const int X_LINE_THICKNESS = 1;
        private const double Y_LINE_THICKNESS = 0.5;
        private const int Y_NUMBER_VERTICAL_OFFSET = 20;
        private const int Y_NUMBER_HORIZONTAL_OFFSET = 8;
        private const int SUMMARISER_TEXT_OFFSET = 5;

        private double ymin = YMIN_DEFAULT;//Dependant on how many lower items there are (no larger if there are > 4) and if the activity summariser is enabled
        private List<GraphDataset> datasets;
        private int xDivisor = 1;
        private int yDivisor = 1;
        private string xAxisName = "";
        private string yAxisName = "";
        private bool summariser = true;

        public int XDivisor { get { return xDivisor; } set { xDivisor = value; } }
        public int YDivisor { get { return yDivisor; } set { yDivisor = value; } }
        public string XAxisName { set { xAxisName = value; } }
        public string YAxisName { set { yAxisName = value; } }

        static TestGraph()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TestGraph), new FrameworkPropertyMetadata(typeof(TestGraph)));
        }

        public void DisableSummariser()
        {
            summariser = false;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        public void AddDataset(GraphDataset data)
        {
            if (datasets == null) datasets = new List<GraphDataset>();
            if (!datasets.Contains(data) && datasets.Count < 5) datasets.Add(data);
        }

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
                    List<string> topData = DrawSummariserLayout(nodeNames);
                    DrawSummariserData(topData, datasets, xdatamax);
                }
            }

            DrawXAxis(xdatamax);
            DrawYAxis(ydatamax);

            if (xAxisName != "") DrawGraphText(xAxisName, Width / 2, (Height - ymin + NAME_HORIZONTAL_OFFSET));
            if (yAxisName != "") DrawGraphText(yAxisName, 0, ((Height - ymin) / 2) - TEXT_HEIGHT / 2);
            if (datasets != null) DrawDatasets(xdatamax, ydatamax);
        }

        private void DrawSummariserData(List<string> topData, List<GraphDataset> datasets, double xdatamax)
        {
            double itemHeight = YMIN_INCREMENTSIZE / datasets.Count();
            for (int e = 0; e < datasets.Count(); e++)
            {
                GraphDataset dataset = datasets[e];
                for (int i = 0; i < dataset.Nodes.Count(); i++)
                {
                    string itemName = dataset.Nodes[i].NodeName;
                    Rectangle rect = new Rectangle();
                    rect.Height = itemHeight;
                    double xpoint1 = (((Width - XMIN) / xdatamax) * dataset.Nodes[i].GetCoords()[0]) + XMIN;
                    double xpoint2 = Width;
                    if (i + 1 < dataset.Nodes.Count())
                        xpoint2 = (((Width - XMIN) / xdatamax) * dataset.Nodes[i + 1].GetCoords()[0]) + XMIN;
                    rect.Width = xpoint2 - xpoint1;
                    rect.Fill = dataset.Colour;
                    rect.StrokeThickness = 0;
                    rect.ToolTip = dataset.Nodes[i].NodeName;
                    if (topData.Contains(itemName))
                    {
                        int startingPosition = TEXT_HEIGHT * 2;
                        if (topData.Count() == NUMBEROFYMININCREMENTS)
                        {
                            startingPosition = TEXT_HEIGHT;
                        }
                        double v = Height - startingPosition - (TEXT_HEIGHT * topData.IndexOf(itemName)) + (itemHeight * e);
                        SetTop(rect, v);
                    }
                    else
                    {
                        //TODO set color depending on what item it is
                        SetTop(rect, Height - TEXT_HEIGHT + (itemHeight * e));
                    }
                    SetLeft(rect, xpoint1 - BUTTON_SIZE / 2);
                    Children.Add(rect);
                }
            }

        }

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

        private void DrawButtonsForDataset(GraphDataset data)
        {
            foreach (GraphNode node in data.Nodes)
            {
                node.SetButtonStyle(FindResource("CircleButton") as Style);
                Children.Add(node.NodeButton);
            }
        }

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

        private Point SetupNodePosition(double xdatamax, double ydatamax, GraphNode node)
        {
            double xpoint = (((Width - XMIN) / xdatamax) * node.GetCoords()[0]) + XMIN;
            double ypoint = (Height - ymin) - (((Height - ymin) / ydatamax) * node.GetCoords()[1]);
            SetLeft(node.NodeButton, xpoint - BUTTON_SIZE / 2);
            SetTop(node.NodeButton, ypoint - BUTTON_SIZE / 2);
            return new Point(xpoint, ypoint);
        }

        private List<string> DrawSummariserLayout(Dictionary<string, int> nodeNames)
        {
            List<string> topData = new List<string>();
            int currentPosition = 0;
            int maxIteration = nodeNames.Count();

            GeometryGroup xaxis_geom = new GeometryGroup();
            if (nodeNames.Count() > 4)
            {
                maxIteration = 4;
                xaxis_geom.Children.Add(new LineGeometry(new Point(0, Height), new Point(Width, Height)));
                DrawGraphText("Other", SUMMARISER_TEXT_OFFSET, Height - TEXT_HEIGHT);
                currentPosition++;
            }
            for (int i = currentPosition; i < maxIteration; i++)
            {
                int max = 0;
                string maxName = "";
                foreach (KeyValuePair<string, int> item in nodeNames)
                {
                    string key = item.Key;
                    int val = item.Value;
                    if (!topData.Contains(key) && val > max)
                    {
                        max = val;
                        maxName = key;
                    }
                }
                topData.Add(maxName);
                double height = Height - (i * YMIN_INCREMENTSIZE);
                DrawGraphText(maxName, SUMMARISER_TEXT_OFFSET, height - TEXT_HEIGHT);
                xaxis_geom.Children.Add(new LineGeometry(new Point(0, height), new Point(Width, height)));
                currentPosition++;
            }
            double topBorderHeight = Height - ((currentPosition) * YMIN_INCREMENTSIZE);
            xaxis_geom.Children.Add(new LineGeometry(new Point(0, topBorderHeight), new Point(Width, topBorderHeight)));
            DrawLines(xaxis_geom, 1, Brushes.LightGray);
            return topData;

        }

        private int AddDataToKey(int keyOffset, GraphDataset data)
        {
            Rectangle rect = new Rectangle();
            Label lab = new Label();

            rect.Fill = data.Colour;
            rect.StrokeThickness = KEY_ICON_BORDER;
            rect.Height = KEY_ICONSIZE;
            rect.Width = KEY_ICONSIZE;
            lab.Content = data.DatasetName;

            SetTop(rect, Height - ymin - keyOffset);
            SetTop(lab, Height - ymin - keyOffset);
            SetLeft(lab, KEY_ICONSIZE);

            Children.Add(rect);
            Children.Add(lab);
            return keyOffset + KEY_ICONSIZE + KEY_GAP;
        }

        private Polyline GenerateGraphLine(Brush brush, PointCollection points)
        {
            Polyline polyline = new Polyline();
            polyline.StrokeThickness = GRAPH_LINE_SIZE;
            polyline.Stroke = brush;
            polyline.Points = points;
            return polyline;

        }

        private void DrawXAxis(double xdatamax)
        {
            GeometryGroup xaxis_geom = new GeometryGroup();
            xaxis_geom.Children.Add(new LineGeometry(new Point(XMIN, Height - ymin), new Point(Width, Height - ymin)));
            for (double x = 0; x <= xdatamax; x += xDivisor)
            {
                double point = (((Width - XMIN) / xdatamax) * (x)) + XMIN;
                xaxis_geom.Children.Add(new LineGeometry(new Point(point, Height - ymin - X_LINE_MARKE_RSIZE), new Point(point, Height - ymin)));
                DrawGraphText(x.ToString(), point - 2, (Height - ymin));
            }
            DrawLines(xaxis_geom, X_LINE_THICKNESS, Brushes.Black);
        }

        private void DrawYAxis(double ydatamax)
        {
            GeometryGroup yaxis_geom = new GeometryGroup();
            for (double y = 0; y <= ydatamax; y += yDivisor)
            {
                if (y != 0)
                {
                    double point = (Height - ymin) - (((Height - ymin) / ydatamax) * (y));
                    yaxis_geom.Children.Add(new LineGeometry(new Point(Width, point), new Point(XMIN, point)));
                    DrawGraphText(y.ToString(), XMIN - Y_NUMBER_VERTICAL_OFFSET, (point - Y_NUMBER_HORIZONTAL_OFFSET));
                }
            }
            DrawLines(yaxis_geom, Y_LINE_THICKNESS, Brushes.Gray);
        }

        private void DrawGraphText(string x, double LeftPoint, double TopPoint)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = x;
            textBlock.Foreground = Brushes.Black;
            SetLeft(textBlock, LeftPoint);
            SetTop(textBlock, TopPoint);
            Children.Add(textBlock);
        }

        private void DrawLines(GeometryGroup axis_geom, double thickness, SolidColorBrush brush)
        {
            Path axis_path = new Path();
            axis_path.StrokeThickness = thickness;
            axis_path.Stroke = brush;
            axis_path.Data = axis_geom;
            Children.Add(axis_path);
        }

    }
}
