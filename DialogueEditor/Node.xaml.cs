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
	/// Interaction logic for Node.xaml
	/// </summary>
	public partial class Node : UserControl
	{
		protected Point dragOffset;

		public Node()
		{
			InitializeComponent();
			Width = grid.Width;
			Height = grid.Height;
		}

		public void SetPosition(double x, double y)
		{
			Canvas.SetLeft(this, x);
			Canvas.SetTop(this, y);
		}

		public Point GetPosition()
		{
			return new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
		}

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			base.OnMouseDown(e);
			Console.WriteLine("Down");
			CaptureMouse();
			MouseMove += OnNodeDragged;

			var mousePos = e.GetPosition((IInputElement)Parent);
			dragOffset =  GetPosition() - (Vector)mousePos;
		}


		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			base.OnMouseUp(e);
			Console.WriteLine("Up");
			ReleaseMouseCapture();
			MouseMove -= OnNodeDragged;
		}

		protected void OnNodeDragged(object sender, MouseEventArgs e)
		{
			var mousePos = e.GetPosition((IInputElement)Parent);
			mousePos = mousePos + (Vector)dragOffset;
			SetPosition(mousePos.X, mousePos.Y);
		}
	}
}
