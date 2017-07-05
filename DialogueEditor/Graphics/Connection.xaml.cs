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
	/// Interaction logic for Connection.xaml
	/// </summary>
	public partial class Connection : UserControl
	{
		public Node objFrom, objTo;
		private Point fromSavedPosition = new Point(double.NaN, double.NaN), toSavedPosition = new Point(double.NaN, double.NaN);
		public double pinOffset;

		private Point[] line1Points = { new Point(), new Point(), new Point() }, line2Points = { new Point(), new Point(), new Point() };
		private PolyBezierSegment line1, line2;
		private PathFigure fig;
		private Path path = new Path();
		private Pen pen;

		public Connection(Node from, Node to)
		{
			InitializeComponent();
			this.objFrom = from;
			this.objTo = to;
			path.Stroke = new SolidColorBrush(Colors.Black);
			path.StrokeThickness = 5;
			pen = new Pen(new SolidColorBrush(Colors.Black), 5);
			objFrom.allConnections.Add(this);
			objTo.allConnections.Add(this);
		}

		private static Point GetObjectPosition(FrameworkElement element)
		{
			return new Point(Canvas.GetLeft(element), Canvas.GetTop(element));
		}
		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);
			if(fromSavedPosition!=GetObjectPosition(objFrom) || toSavedPosition!=GetObjectPosition(objTo))
			{
				//Set variables to new values here
				fromSavedPosition = GetObjectPosition(objFrom);
				toSavedPosition = GetObjectPosition(objTo);

				Point pFrom = new Point(fromSavedPosition.X + objFrom.ActualWidth / 2, fromSavedPosition.Y + objFrom.ActualHeight);
				Point pTo = new Point(toSavedPosition.X + objTo.ActualWidth / 2, toSavedPosition.Y);
				Point pMiddle = new Point((pFrom.X + pTo.X) / 2, (pFrom.Y + pTo.Y) / 2);

				line1Points[0] = pFrom;
				line1Points[1] = new Point(pFrom.X, pFrom.Y + pinOffset);
				line1Points[2] = pMiddle;

				line2Points[0] = pMiddle;
				line2Points[1] = new Point(pTo.X, pTo.Y - pinOffset);
				line2Points[2] = pTo;

				line1 = new PolyBezierSegment(line1Points, true);
				line2 = new PolyBezierSegment(line2Points, true);
				fig = new PathFigure(pFrom, new PathSegment[] { line1, line2 }, false);

				path.Data = new PathGeometry(new PathFigure[] { fig });
			}
			//Render here
			drawingContext.DrawLine(pen,
				new Point(fromSavedPosition.X + objFrom.ActualWidth / 2, fromSavedPosition.Y + objFrom.ActualHeight),
				new Point(toSavedPosition.X + objTo.ActualWidth / 2, toSavedPosition.Y));
		}
	}
}
