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

        private double ymin = 115;//dependant on how many lower items there are (no larger if there are > 4) and if the activity summariser is enabled
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
            datasets.Add(data);
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
                //DrawSummariser(nodeNames);
            }

            DrawXAxis(xdatamax);
            DrawYAxis(ydatamax);

            if (xAxisName != "") DrawGraphText(xAxisName, Width / 2, (Height - ymin + 15));
            if (yAxisName != "") DrawGraphText(yAxisName, 0, ((Height - ymin) / 2) - 10);
            if (datasets != null) DrawDatasets(xdatamax, ydatamax);
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
            Polyline polyline = GeneratePolyline(data.Colour, points);
            Children.Add(polyline);
        }

        private Point SetupNodePosition(double xdatamax, double ydatamax, GraphNode node)
        {
            double xpoint = (((Width - XMIN) / xdatamax) * node.GetCoords()[0]) + XMIN;
            double ypoint = (Height - ymin) - (((Height - ymin) / ydatamax) * node.GetCoords()[1]);
            SetLeft(node.NodeButton, xpoint - 5);
            SetTop(node.NodeButton, ypoint - 5);
            return new Point(xpoint, ypoint);
        }

        private void DrawSummariser(Dictionary<string, int> nodeNames)
        {
            ymin = 110;
            if (summariser == false)
            {
                ymin = 30;
                return;
            }
            ymin -= (20 * (4 - nodeNames.Count()));

            //draw summariser

        }

        private int AddDataToKey(int keyOffset, GraphDataset data)
        {
            Rectangle rect = new Rectangle();
            Label lab = new Label();

            rect.Fill = data.Colour;
            rect.StrokeThickness = 0;
            rect.Height = 20;
            rect.Width = 20;
            lab.Content = data.DatasetName;

            SetTop(rect, Height - ymin - keyOffset);
            SetTop(lab, Height - ymin - keyOffset);
            SetLeft(lab, 20);

            Children.Add(rect);
            Children.Add(lab);
            return keyOffset + 23;
        }

        private static Polyline GeneratePolyline(Brush brush, PointCollection points)
        {
            Polyline polyline = new Polyline();
            polyline.StrokeThickness = 3;
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
                xaxis_geom.Children.Add(new LineGeometry(new Point(point, Height - ymin - 5), new Point(point, Height - ymin)));
                DrawGraphText(x.ToString(), point - 2, (Height - ymin));
            }
            DrawLine(xaxis_geom, 1, Brushes.Black);
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
                    DrawGraphText(y.ToString(), XMIN - 20, (point - 8));
                }
            }
            DrawLine(yaxis_geom, 0.5, Brushes.Gray);
        }

        private void DrawGraphText(string x, double LeftPoint, double TopPoint)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = x;
            textBlock.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            SetLeft(textBlock, LeftPoint);
            SetTop(textBlock, TopPoint);
            Children.Add(textBlock);
        }

        private void DrawLine(GeometryGroup axis_geom, double thickness, SolidColorBrush brush)
        {
            Path axis_path = new Path();
            axis_path.StrokeThickness = thickness;
            axis_path.Stroke = brush;
            axis_path.Data = axis_geom;
            Children.Add(axis_path);
        }

    }
}
