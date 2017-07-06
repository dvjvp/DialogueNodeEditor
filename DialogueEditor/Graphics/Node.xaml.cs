using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DialogueEditor.Files;

namespace DialogueEditor
{
	/// <summary>
	/// Interaction logic for Node.xaml
	/// </summary>
	public partial class Node : UserControl
	{
		protected Point dragOffset;
		private static Action emptyDelegate = delegate { };

		public DialogueDataLine sourceData;

		public List<Connection> allConnections = new List<Connection>();
		public List<Connection> outputConnections = new List<Connection>();

		public Node(DialogueDataLine sourceData)
		{
			InitializeComponent();
			Width = grid.Width;
			Height = grid.Height;
			this.sourceData = sourceData;
			LoadDataFromSource();
		}

		public void LoadDataFromSource()
		{
			nodeNameField.Text = sourceData.rowName;
			dialogueText.Text = sourceData.dialogueText;
			SetPosition(sourceData.nodePositionX, sourceData.nodePositionY);
			switch(sourceData.command)
			{
				case "leave":
					outputType.Text = "End dialogue";
					break;
				case "options":
					outputType.Text = "Multiple choices";
					break;
				case "has-item":
					outputType.Text = "If player has item";
					string[] _s = sourceData.commandArguments.Split(' ');
					itemName.Text = _s[0];
					try
					{
						itemCount.Text = _s[1];
					}
					catch (Exception)
					{
						itemCount.Text = "1";
					}
					break;
				case "actor-message":
					outputType.Text = "Call actor event";
					try
					{
						string[] s = sourceData.commandArguments.Split(' ');
						actorName.Text = s[0];
						actorEventName.Text = s[1];
					}
					catch (Exception)
					{
					}
					break;
				case "level-message":
					outputType.Text = "Call level event";
					levelEventName.Text = sourceData.commandArguments;
					break;
				default:
					outputType.Text = "Normal dialogue";
					break;
			}
		}

		public void LoadOutputConnectionDataFromSource()
		{
			throw new NotImplementedException();
		}

		public void DeleteAllOutputConnections()
		{
			throw new NotImplementedException();
		}

		public void SetPosition(double x, double y)
		{
			Canvas.SetLeft(this, x);
			Canvas.SetTop(this, y);
			sourceData.SetPosition(x, y);
		}

		public Point GetPosition()
		{
			return new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
		}
#region Drag'n'Drop node

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
			ForceConnectionUpdate();
		}

		protected void ForceConnectionUpdate()
		{
			//(Parent as Canvas).Dispatcher.Invoke(emptyDelegate, System.Windows.Threading.DispatcherPriority.Render);
			foreach (var connection in allConnections)
			{
				connection.InvalidateVisual();
			}
		}

#endregion

		private void ButtonDelete_Click(object sender, RoutedEventArgs e)
		{
			Delete();
		}

		public void Delete()
		{
			Console.WriteLine("Deleting node: " + nodeNameField.Text);
			MainWindow.instance.DeleteNode(this);
		}

		private void dialogueText_TextChanged(object sender, TextChangedEventArgs e)
		{

		}
	}
}
