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
using System.Drawing;

namespace DialogueEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool isDragNDrop = false;
        Rectangle rect;
        Button butt;
        public MainWindow()
        {
            InitializeComponent();


            rect = new Rectangle();
            rect.Width = 100;    rect.Height = 100;
            rect.Fill = new SolidColorBrush(Colors.Blue);
            rect.Stroke = new SolidColorBrush(Colors.Black);
//             rect.MouseDown += Rect_MouseDown;
//             rect.MouseUp += DrawArea_MouseUp;
//             rect.MouseMove += Rect_MouseMove;

            drawArea.Children.Add(rect);
            Canvas.SetLeft(rect, 10);
            Canvas.SetTop(rect, 10);

            butt = new Button();
            butt.Width = 100; butt.Height = 100;
            butt.Background = new SolidColorBrush(Colors.Blue);
            butt.Foreground = new SolidColorBrush(Colors.Black);

            drawArea.Children.Add(butt);
            Canvas.SetLeft(butt, 210);
            Canvas.SetTop(butt, 210);

            DrawNodeCurve();

            //https://forum.unity3d.com/threads/simple-node-editor.189230/

            //             drawArea.MouseUp += DrawArea_MouseUp;
            //             drawArea.MouseMove += Rect_MouseMove;
        }

        void DrawNodeCurve()
        {
			//             NodeGraphics.BezierFigure line = new NodeGraphics.BezierFigure();
			//             line.StartPoint = new Point(Canvas.GetLeft(rect) + rect.ActualWidth / 2, Canvas.GetTop(rect) + rect.ActualHeight);
			//             line.EndPoint = new Point(Canvas.GetLeft(butt) + rect.ActualWidth / 2, Canvas.GetTop(rect));
			//             line.StartBezierPoint = new Point(Canvas.GetLeft(rect) + rect.ActualWidth / 2, Canvas.GetTop(rect) + rect.ActualHeight + NodeGraphics.BezierFigure.TangentOffset);
			//             line.EndBezierPoint = new Point(Canvas.GetLeft(butt) + rect.ActualWidth / 2, Canvas.GetTop(rect) - NodeGraphics.BezierFigure.TangentOffset);
			// 
			//             line.Background = new SolidColorBrush(Colors.Black);
			//             line.BorderBrush = new SolidColorBrush(Colors.Red);
			// 
			//             drawArea.Children.Add(line);

			Point from = new Point(Canvas.GetLeft(rect) + rect.Width / 2, Canvas.GetTop(rect) + rect.Height);
			Point to = new Point(Canvas.GetLeft(butt) + butt.Width / 2, Canvas.GetTop(butt));
			Point middlePoint = new Point((from.X + to.X) / 2, (from.Y + to.Y) / 2);

			Point[] linePoints = new Point[]
            {
                from,
                new Point(from.X, from.Y + 80),
				middlePoint
				//new Point(Canvas.GetLeft(butt) + butt.Width / 2, Canvas.GetTop(butt)),
                //new Point(90,200), new Point(140,200), new Point(160,200), new Point(180,200), new Point(430,190), new Point(430,280)
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

        //         private void Rect_MouseMove(object sender, MouseEventArgs e)
        //         {
        //             if (!isDragNDrop) return;
        // 
        //             var mousePos = e.GetPosition(drawArea);
        // 
        //             double left = mousePos.X - (rect.ActualWidth / 2), top = mousePos.Y - (rect.ActualHeight / 2);
        //             Canvas.SetLeft(rect, left);
        //             Canvas.SetTop(rect, top);
        //         }
        // 
        //         private void DrawArea_MouseUp(object sender, MouseButtonEventArgs e)
        //         {
        //             Console.WriteLine("World");
        //             isDragNDrop = false;
        //         }
        // 
        //         private void Rect_MouseDown(object sender, MouseButtonEventArgs e)
        //         {
        //             Console.WriteLine("Hello");
        //             isDragNDrop = true;
        //         }
    }
}
