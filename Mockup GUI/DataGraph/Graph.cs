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
    public abstract class Graph : Canvas
    {

        private const int DEFAULT_RECTANGLE_BORDER = 0;
        private const int GRAPH_LINE_SIZE = 3;

        protected static readonly Brush[] otherBrushes = new Brush[]{ Brushes.MediumTurquoise, Brushes.LightGreen, Brushes.Gray, Brushes.Salmon,
             Brushes.Orange, Brushes.Crimson, Brushes.Black, Brushes.SteelBlue, Brushes.Orange, Brushes.Tan};
        static Graph()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Graph), new FrameworkPropertyMetadata(typeof(Graph)));
        }

        /// <summary>
        /// Generates a TextBlock from the supplied string at the supplied positions
        /// </summary>
        /// <param name="text">the text to be written into the TextBlock</param>
        /// <param name="x">the x position of the TextBlock</param>
        /// <param name="y">the y position of the TextBlock</param>
        /// <returns></returns>
        protected TextBlock GenerateGraphText(string text, double x, double y)
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
        protected Path GenerateSetOfLines(GeometryGroup lines, double thickness, SolidColorBrush brush)
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
        protected static Rectangle GenerateRectangle(double height, double width, Brush colour, string tooltip)
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
        protected Polyline GenerateGraphLine(Brush brush, PointCollection points)
        {
            Polyline polyline = new Polyline();
            polyline.StrokeThickness = GRAPH_LINE_SIZE;
            polyline.Stroke = brush;
            polyline.Points = points;
            return polyline;

        }

    }
}
