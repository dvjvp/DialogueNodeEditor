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

namespace DialogueEditor.Graphics
{
	/// <summary>
	/// Interaction logic for Comment.xaml
	/// </summary>
	public partial class Comment : UserControl
	{
		bool dragndropInProgress = false;
		Vector dragndropOffset;
		Vector initialSize;

		public Comment()
		{
			InitializeComponent();

			//position and size have to be set before retrieving it, otherwise it would be NaN
			Width = 300;
			Height = 300;
			SetPosition(new Point(0, 0));
		}

		#region Transformations
		public Point GetPosition()
		{
			return new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
		}
		public void SetPosition(Point newPosition)
		{
			Canvas.SetLeft(this, newPosition.X);
			Canvas.SetTop(this, newPosition.Y);
		}
		#endregion

		#region Drag'n'drop Border - move comment around
		private void DragndropBorder_MouseDown(object sender, MouseButtonEventArgs e)
		{
			dragndropInProgress = true;
			DragndropBorder.CaptureMouse();
			DragndropBorder.MouseMove += DragndropBorder_MouseMove;
			dragndropOffset = GetPosition() - e.GetPosition(MainWindow.instance.drawArea);

		}
		private void DragndropBorder_MouseMove(object sender, MouseEventArgs e)
		{
			Point updatedPosition = e.GetPosition(MainWindow.instance.drawArea) + dragndropOffset;
			SetPosition(updatedPosition);
		}
		private void DragndropBorder_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if(!dragndropInProgress)
			{
				return;
			}
			dragndropInProgress = false;
			DragndropBorder.ReleaseMouseCapture();
			DragndropBorder.MouseMove -= DragndropBorder_MouseMove;
		}
		#endregion

		#region Resizing comment box
		/* RIGHT */
		private void ResizeRight_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Rectangle r = sender as Rectangle;
			dragndropInProgress = true;
			r.CaptureMouse();
			r.MouseMove += ResizeRight_MouseMove;
			dragndropOffset = (Vector)e.GetPosition(MainWindow.instance.drawArea);
			initialSize = new Vector(Width, Height);
		}
		private void ResizeRight_MouseMove(object sender, MouseEventArgs e)
		{
			Rectangle r = sender as Rectangle;
			Point totalChange = e.GetPosition(MainWindow.instance.drawArea) - dragndropOffset + initialSize;
			Width = totalChange.X;
		}
		private void ResizeRight_MouseUp(object sender, MouseButtonEventArgs e)
		{
			Rectangle r = sender as Rectangle;
			if (!dragndropInProgress)
			{
				return;
			}
			dragndropInProgress = false;
			r.ReleaseMouseCapture();
			r.MouseMove -= ResizeRight_MouseMove;
		}

		/* DOWN */
		private void ResizeDown_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Rectangle r = sender as Rectangle;
			dragndropInProgress = true;
			r.CaptureMouse();
			r.MouseMove += ResizeDown_MouseMove;
			dragndropOffset = (Vector)e.GetPosition(MainWindow.instance.drawArea);
			initialSize = new Vector(Width, Height);
		}
		private void ResizeDown_MouseMove(object sender, MouseEventArgs e)
		{
			Rectangle r = sender as Rectangle;
			Point totalChange = e.GetPosition(MainWindow.instance.drawArea) - dragndropOffset + initialSize;
			Height = totalChange.Y;
		}
		private void ResizeDown_MouseUp(object sender, MouseButtonEventArgs e)
		{
			Rectangle r = sender as Rectangle;
			if (!dragndropInProgress)
			{
				return;
			}
			dragndropInProgress = false;
			r.ReleaseMouseCapture();
			r.MouseMove -= ResizeDown_MouseMove;
		}

		/* DOWN-RIGHT */
		private void ResizeDownRight_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Rectangle r = sender as Rectangle;
			dragndropInProgress = true;
			r.CaptureMouse();
			r.MouseMove += ResizeRight_MouseMove;
			r.MouseMove += ResizeDown_MouseMove;
			dragndropOffset = (Vector)e.GetPosition(MainWindow.instance.drawArea);
			initialSize = new Vector(Width, Height);
		}
		private void ResizeDownRight_MouseUp(object sender, MouseButtonEventArgs e)
		{
			Rectangle r = sender as Rectangle;
			if (!dragndropInProgress)
			{
				return;
			}
			dragndropInProgress = false;
			r.ReleaseMouseCapture();
			r.MouseMove -= ResizeRight_MouseMove;
			r.MouseMove -= ResizeDown_MouseMove;
		}

		/* UP */
		private void ResizeUp_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Rectangle r = sender as Rectangle;
			dragndropInProgress = true;
			r.CaptureMouse();
			r.MouseMove += ResizeUp_MouseMove;
			dragndropOffset = GetPosition() - e.GetPosition(MainWindow.instance.drawArea);
			initialSize = new Vector(Width, Height);
		}
		private void ResizeUp_MouseMove(object sender, MouseEventArgs e)
		{
			Rectangle r = sender as Rectangle;
			Point updatedPosition = e.GetPosition(MainWindow.instance.drawArea) + dragndropOffset;
			double bottom = Canvas.GetTop(this) + Height;
			Canvas.SetTop(this, updatedPosition.Y);
			Height = bottom - updatedPosition.Y;
		}
		private void ResizeUp_MouseUp(object sender, MouseButtonEventArgs e)
		{
			Rectangle r = sender as Rectangle;
			if (!dragndropInProgress)
			{
				return;
			}
			dragndropInProgress = false;
			r.ReleaseMouseCapture();
			r.MouseMove -= ResizeUp_MouseMove;
		}

		#endregion
	}
}
