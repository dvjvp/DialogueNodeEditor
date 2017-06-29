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

			DrawConnection(rect, butt);

            //https://forum.unity3d.com/threads/simple-node-editor.189230/

        }

        void DrawConnection(FrameworkElement fromObj, FrameworkElement toObj)
        {

			Point from = new Point(Canvas.GetLeft(fromObj) + fromObj.Width / 2, Canvas.GetTop(fromObj) + fromObj.Height);
			Point to = new Point(Canvas.GetLeft(toObj) + toObj.Width / 2, Canvas.GetTop(toObj));
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
