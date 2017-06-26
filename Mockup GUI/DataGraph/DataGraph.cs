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
        const double XMIN = 60;

        private double ymin = 110;//dependant on how many lower items there are (no larger if there are > 4) and if the activity summariser is enabled
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
            List<string> nodeNames = new List<string>();

            if (datasets != null)
            {
                foreach (GraphDataset data in datasets)
                {
                    foreach (GraphNode node in data.Nodes)
                    {
                        double[] XY = node.GetCoords();
                        if (XY[0] > xdatamax) xdatamax = XY[0];
                        if (XY[1] > ydatamax) ydatamax = XY[1];
                        if (!nodeNames.Contains(node.NodeName)) nodeNames.Add(node.NodeName);
                    }
                }
                xdatamax = Math.Ceiling(xdatamax / xDivisor) * xDivisor;
                ydatamax = Math.Ceiling(ydatamax / yDivisor) * yDivisor;

                //TO BE USED WHEN THE BOTTOM HALF IS MADE DYNAMIC
                //if( summariser == false){
                //    ymin = 30;
                //}else if (nodeNames.Count() < 4 )
                //{
                //    ymin -= (20 * (4 - nodeNames.Count()));
                //}
                //TODO draw each dataset onto the graph
            }

            DrawXAxis(xdatamax);
            DrawYAxis(ydatamax);
            if (xAxisName != "") DrawGraphText(xAxisName, Width / 2, (Height - ymin + 10));
            if (yAxisName != "") DrawGraphText(yAxisName, 0, ((Height - ymin) / 2) - 10);
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
