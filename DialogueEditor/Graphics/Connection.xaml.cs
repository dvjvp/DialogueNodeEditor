using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DialogueEditor
{
	/// <summary>
	/// Interaction logic for Connection.xaml
	/// </summary>
	public partial class Connection : UserControl
	{
		public FrameworkElement objFrom, objTo;
		public Node parentFrom, parentTo;
		private Point fromSavedPosition = new Point(double.NaN, double.NaN), toSavedPosition = new Point(double.NaN, double.NaN);
		public double pinOffset = 180;

		private Point[] line1Points = { new Point(), new Point(), new Point() }, line2Points = { new Point(), new Point(), new Point() };
		private PolyBezierSegment line1, line2;
		private PathFigure fig;
		private PathGeometry path;
		private Pen pen;
		private Brush _brush;
		public Brush Brush
		{
			get
			{
				return _brush;
			}
			set
			{
				_brush = value;
				pen = new Pen(Brush, 5);
			}
		}

		public Connection(Node parentFrom, FrameworkElement inputFrom, Node to)
		{
			InitializeComponent();
			this.parentFrom = parentFrom;
			objFrom = inputFrom;
			parentTo = to;
			objTo = to.InputPin;
			Brush = new SolidColorBrush(Colors.Black);
			//pen = new Pen(brush, 5);

			MouseEnter += Connection_MouseEnter;
			MouseLeave += Connection_MouseLeave;
		}

		private void Connection_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			SetHighlightEnabled(false);
		}

		private void Connection_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			SetHighlightEnabled(true);
		}

		public void SetHighlightEnabled(bool highlighted)
		{
			Brush = highlighted ? Brushes.AliceBlue : Brushes.Black;
			InvalidateVisual();
		}

		private static Point GetObjectPosition(FrameworkElement element)
		{
			return new Point(Canvas.GetLeft(element), Canvas.GetTop(element));
		}
		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);


			if (fromSavedPosition != GetObjectPosition(objFrom) || toSavedPosition != GetObjectPosition(objTo))
			{
				//Set variables to new values here
				fromSavedPosition = GetObjectPosition(parentFrom) + (Vector)objFrom.TransformToAncestor(parentFrom).Transform(new Point(0, 0));
				toSavedPosition = GetObjectPosition(parentTo) + (Vector)objTo.TransformToAncestor(parentTo).Transform(new Point(0, 0));

				Point pFrom = new Point(fromSavedPosition.X + objFrom.ActualWidth / 2, fromSavedPosition.Y + objFrom.ActualHeight);
				Point pTo = new Point(toSavedPosition.X + objTo.ActualWidth / 2, toSavedPosition.Y);
				Point pMiddle = new Point((pFrom.X + pTo.X) / 2, (pFrom.Y + pTo.Y) / 2);


				double distance = ((Vector)(pTo - (Vector)pFrom)).Length;
				pinOffset = distance / 3;

				line1Points[0] = pFrom;
				line1Points[1] = new Point(pFrom.X, pFrom.Y + pinOffset);
				line1Points[2] = pMiddle;

				line2Points[0] = pMiddle;
				line2Points[1] = new Point(pTo.X, pTo.Y - pinOffset);
				line2Points[2] = pTo;

				line1 = new PolyBezierSegment(line1Points, true);
				line2 = new PolyBezierSegment(line2Points, true);
				fig = new PathFigure(pFrom, new PathSegment[] { line1, line2 }, false);

				path = new PathGeometry(new PathFigure[] { fig });
			}
			// 			drawingContext.DrawLine(pen,
			// 				new Point(fromSavedPosition.X + objFrom.ActualWidth / 2, fromSavedPosition.Y + objFrom.ActualHeight),
			// 				new Point(toSavedPosition.X + objTo.ActualWidth / 2, toSavedPosition.Y));
			drawingContext.DrawGeometry(null, pen, path);
		}
	}
}
