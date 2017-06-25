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
        const double YMIN = 110;//dependant on how many lower items there are and if it is enabled

        private List<GraphDataset> datasets;
        private int xDivisor = 1;
        private int yDivisor = 10;
        private string xAxisName = "";
        private string yAxisName = "";

        public int XDivisor { get { return xDivisor; } set { xDivisor = value; } }
        public int YDivisor { get { return yDivisor; } set { yDivisor = value; } }
        public string XAxisName { set { xAxisName = value; } }
        public string YAxisName { set { yAxisName = value; } }
        static TestGraph()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TestGraph), new FrameworkPropertyMetadata(typeof(TestGraph)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            datasets = new List<GraphDataset>();

        }

        public void AddDataset(GraphDataset data)
        {
            datasets.Add(data);
        }
       
        public void DrawGraph()
        {
            double xdatamax = 20;//largest x value in datasets 
            double ydatamax = 80;//largest y value in datasets
            DrawXAxis(xdatamax);
            DrawYAxis(ydatamax);
            if (xAxisName != "") DrawGraphText(xAxisName, Width / 2, (Height - YMIN + 10));
            if (yAxisName != "") DrawGraphText(yAxisName, 0, ((Height - YMIN) / 2) - 10);
            // the max x and y are set by the largest var in datasets
        }

        private void DrawXAxis(double xdatamax)
        {
            GeometryGroup xaxis_geom = new GeometryGroup();
            xaxis_geom.Children.Add(new LineGeometry(new Point(XMIN, Height - YMIN), new Point(Width, Height - YMIN)));
            for (double x = 0; x <= xdatamax; x += xDivisor)
            {
                double point = (((Width - XMIN) / xdatamax) * (x)) + XMIN;
                xaxis_geom.Children.Add(new LineGeometry(new Point(point, Height - YMIN - 5), new Point(point, Height - YMIN)));
                DrawGraphText(x.ToString(), point - 2, (Height - YMIN));
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
                    double point = (Height - YMIN) - (((Height - YMIN) / ydatamax) * (y));
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
