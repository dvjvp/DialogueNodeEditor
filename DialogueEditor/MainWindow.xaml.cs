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

namespace DialogueEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool isDragNDrop = false;


        public MainWindow()
        {
            InitializeComponent();
			
			Node node1 = new Node(), node2 = new Node();
			drawArea.Children.Add(node1);
			drawArea.Children.Add(node2);

			node1.SetPosition(100, 100);
			node2.SetPosition(300, 300);

			

			DrawConnection(node1, node2);

            //https://forum.unity3d.com/threads/simple-node-editor.189230/

        }

        void DrawConnection(FrameworkElement fromObj, FrameworkElement toObj)
        {
			Point from = new Point(Canvas.GetLeft(fromObj) + fromObj.ActualWidth / 2, Canvas.GetTop(fromObj) + fromObj.ActualHeight);
			Point to = new Point(Canvas.GetLeft(toObj) + toObj.ActualWidth / 2, Canvas.GetTop(toObj));
			Point middlePoint = new Point((from.X + to.X) / 2, (from.Y + to.Y) / 2);

			Point[] linePoints = new Point[]
            {
                from,
                new Point(from.X, from.Y + 80),
				middlePoint
            };
			Point[] linePoints2 = new Point[]
			{
				middlePoint,
				new Point(to.X, to.Y - 80),
				to
			};
            PolyBezierSegment line = new PolyBezierSegment(linePoints, true);
			PolyBezierSegment line2 = new PolyBezierSegment(linePoints2, true);
            PathFigure fig = new PathFigure(from, new PathSegment[] { line, line2 }, false);
            
            Path path = new Path();
            path.Data = new PathGeometry(new PathFigure[] { fig });
            path.Stroke = new SolidColorBrush(Colors.Black);
            path.StrokeThickness = 5;
            drawArea.Children.Add(path);
        }

    }
}
