using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace DialogueEditor.Graphics
{
	/// <summary>
	/// Interaction logic for Comment.xaml
	/// </summary>
	public partial class Comment : UserControl
	{
		const double CommentMinimalWidth = 40;
		const double CommentMinimalHeight = 40;

		bool dragndropInProgress = false;
		Vector dragndropOffset;
		Vector initialSize;

		#region Creation and destruction
		protected Comment()
		{
			InitializeComponent();

			//position and size have to be set before retrieving it, otherwise it would be NaN
		}
				
		public static Comment Create()
		{
			return Create(Rect.Empty);
		}
		public static Comment Create(Rect encapsulation)
		{
			if (encapsulation == Rect.Empty)
			{
				encapsulation = new Rect(MainWindow.instance.GetDrawAreaViewCenter(), new Size(300, 300));
			}
			encapsulation.Width = Math.Max(encapsulation.Width, CommentMinimalWidth);
			encapsulation.Height = Math.Max(encapsulation.Height, CommentMinimalHeight);

			Comment c = new Comment();
			MainWindow.instance.drawArea.Children.Insert(0, c);

			double heightOffset = /*c.DragndropBorder.ActualHeight*/ 20;
			double margin = 25;
			Point position = encapsulation.Location - new Vector(margin, heightOffset + margin);

			c.SetPosition(position);
			c.Width = encapsulation.Width + (margin * 2);
			c.Height = encapsulation.Height + heightOffset + (margin * 2);
			return c;
		}

		public static Comment FromDialogueDataLine(Files.DialogueDataLine line)
		{
			Rect r = new Rect();
			r.X = line.nodePositionX;
			r.Y = line.nodePositionY;
			string[] s = line.commandArguments.Split(' ');
			try
			{
				r.Width = double.Parse(s[0]);
				r.Height = double.Parse(s[1]);
			}
			catch (Exception)
			{
				r.Width = 300;
				r.Height = 300;
			}

			Comment c = Create(r);
			c.CommentName.Text = line.prompt;
			return c;
		}
		public Files.DialogueDataLine ToSavableData()
		{
			Files.DialogueDataLine d = new Files.DialogueDataLine();
			d.command = "Comment";

			var position = GetPosition();
			d.nodePositionX = position.X;
			d.nodePositionY = position.Y;

			d.commandArguments = ((int)Width).ToString() + ' ' + ((int)Height).ToString();

			d.prompt = CommentName.Text;

			return d;
		}

		private void DeleteButton_Click(object sender, RoutedEventArgs e)
		{
			MainWindow.instance.DeleteComment(this);
		}
		#endregion

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
			dragndropOffset = GetPosition() - e.GetPosition(MainWindow.instance.drawArea);
			initialSize = new Vector(Width, Height);
		}
		private void ResizeRight_MouseMove(object sender, MouseEventArgs e)
		{
			Rectangle r = sender as Rectangle;
			Vector totalChange = e.GetPosition(MainWindow.instance.drawArea) + dragndropOffset + initialSize - GetPosition();
			Width = Math.Max(totalChange.X, CommentMinimalWidth);
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
			dragndropOffset = GetPosition() - e.GetPosition(MainWindow.instance.drawArea);
			initialSize = new Vector(Width, Height);
		}
		private void ResizeDown_MouseMove(object sender, MouseEventArgs e)
		{
			Rectangle r = sender as Rectangle;
			Vector totalChange = e.GetPosition(MainWindow.instance.drawArea) + dragndropOffset + initialSize - GetPosition();
			Height = Math.Max(totalChange.Y, CommentMinimalHeight);
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
			dragndropOffset = GetPosition() - e.GetPosition(MainWindow.instance.drawArea);
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
			Height = Math.Max(bottom - updatedPosition.Y, CommentMinimalHeight);
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

		/* UP-RIGHT */
		private void ResizeUpRight_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Rectangle r = sender as Rectangle;
			dragndropInProgress = true;
			r.CaptureMouse();
			r.MouseMove += ResizeRight_MouseMove;
			r.MouseMove += ResizeUp_MouseMove;
			dragndropOffset = GetPosition() - e.GetPosition(MainWindow.instance.drawArea);
			initialSize = new Vector(Width, Height);
		}
		private void ResizeUpRight_MouseUp(object sender, MouseButtonEventArgs e)
		{
			Rectangle r = sender as Rectangle;
			if (!dragndropInProgress)
			{
				return;
			}
			dragndropInProgress = false;
			r.ReleaseMouseCapture();
			r.MouseMove -= ResizeRight_MouseMove;
			r.MouseMove -= ResizeUp_MouseMove;
		}

		/* LEFT */
		private void ResizeLeft_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Rectangle r = sender as Rectangle;
			dragndropInProgress = true;
			r.CaptureMouse();
			r.MouseMove += ResizeLeft_MouseMove;
			dragndropOffset = GetPosition() - e.GetPosition(MainWindow.instance.drawArea);
			initialSize = new Vector(Width, Height);
		}
		private void ResizeLeft_MouseMove(object sender, MouseEventArgs e)
		{
			Rectangle r = sender as Rectangle;
			Point updatedPosition = e.GetPosition(MainWindow.instance.drawArea) + dragndropOffset;
			double right = Canvas.GetLeft(this) + Width;
			Canvas.SetLeft(this, updatedPosition.X);
			Width = Math.Max(right - updatedPosition.X, CommentMinimalWidth);
		}
		private void ResizeLeft_MouseUp(object sender, MouseButtonEventArgs e)
		{
			Rectangle r = sender as Rectangle;
			if (!dragndropInProgress)
			{
				return;
			}
			dragndropInProgress = false;
			r.ReleaseMouseCapture();
			r.MouseMove -= ResizeLeft_MouseMove;
		}

		/* UP-LEFT */
		private void ResizeUpLeft_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Rectangle r = sender as Rectangle;
			dragndropInProgress = true;
			r.CaptureMouse();
			r.MouseMove += ResizeLeft_MouseMove;
			r.MouseMove += ResizeUp_MouseMove;
			dragndropOffset = GetPosition() - e.GetPosition(MainWindow.instance.drawArea);
			initialSize = new Vector(Width, Height);
		}
		private void ResizeUpLeft_MouseUp(object sender, MouseButtonEventArgs e)
		{
			Rectangle r = sender as Rectangle;
			if (!dragndropInProgress)
			{
				return;
			}
			dragndropInProgress = false;
			r.ReleaseMouseCapture();
			r.MouseMove -= ResizeLeft_MouseMove;
			r.MouseMove -= ResizeUp_MouseMove;
		}

		/* DOWN-LEFT */
		private void ResizeDownLeft_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Rectangle r = sender as Rectangle;
			dragndropInProgress = true;
			r.CaptureMouse();
			r.MouseMove += ResizeLeft_MouseMove;
			r.MouseMove += ResizeDown_MouseMove;
			dragndropOffset = GetPosition() - e.GetPosition(MainWindow.instance.drawArea);
			initialSize = new Vector(Width, Height);
		}
		private void ResizeDownLeft_MouseUp(object sender, MouseButtonEventArgs e)
		{
			Rectangle r = sender as Rectangle;
			if (!dragndropInProgress)
			{
				return;
			}
			dragndropInProgress = false;
			r.ReleaseMouseCapture();
			r.MouseMove -= ResizeLeft_MouseMove;
			r.MouseMove -= ResizeDown_MouseMove;
		}


		#endregion


	}
}
