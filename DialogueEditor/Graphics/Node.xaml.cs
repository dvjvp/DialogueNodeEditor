using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DialogueEditor.Files;
using System.Windows.Media;

namespace DialogueEditor
{
	/// <summary>
	/// Interaction logic for Node.xaml
	/// </summary>
	public partial class Node : UserControl
	{
		public Vector dragOffset;
		private static Action emptyDelegate = delegate { };

		public DialogueDataLine sourceData;

		public List<Connection> allConnections = new List<Connection>();
		public List<Connection> outputConnections = new List<Connection>();

		private string connectionHasItemTrue, connectionHasItemFalse;

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

		#region Connections

		public void LoadOutputConnectionDataFromSource()
		{
			switch (sourceData.command)
			{
				case "leave":
					break;
				case "options":
					{
						string[] s = sourceData.nextRowName.Split(' ');
						foreach (var item in s)
						{
							try
							{
								Node target = MainWindow.instance.nodeMap[item];
								MakeConnection(target, outputPinMultipleChoices);
							}
							catch (Exception e)
							{
								Console.WriteLine("Exception in LoadOutputConnectionDataFromSource():" + e);
							}
						}
					}
					break;
				case "has-item":
					{
						string[] s = sourceData.nextRowName.Split(' ');
						try
						{
							connectionHasItemTrue = s[0];
							Node target = MainWindow.instance.nodeMap[connectionHasItemTrue];
							MakeConnection(target, outputPinItemTrue);
						}
						catch (Exception e)
						{
							Console.WriteLine("Exception in LoadOutputConnectionDataFromSource():" + e);
						}
						try
						{
							connectionHasItemFalse = s[1];
							Node target = MainWindow.instance.nodeMap[connectionHasItemFalse];
							MakeConnection(target, outputPinItemFalse);
						}
						catch (Exception e)
						{
							Console.WriteLine("Exception in LoadOutputConnectionDataFromSource():" + e);
						}
					}
					break;
				case "actor-message":
					try
					{
						Node target = MainWindow.instance.nodeMap[sourceData.nextRowName];
						MakeConnection(target, outputPinActorEvent);
					}
					catch (Exception e)
					{
						Console.WriteLine("Exception in LoadOutputConnectionDataFromSource():" + e);
					}
					break;
				case "level-message":
					try{
						Node target = MainWindow.instance.nodeMap[sourceData.nextRowName];
						MakeConnection(target, outputPinLevelEvent);
					}
					catch (Exception e)
					{
						Console.WriteLine("Exception in LoadOutputConnectionDataFromSource():" + e);
					}
					break;
				default:
					try{
						Node target = MainWindow.instance.nodeMap[sourceData.nextRowName];
						MakeConnection(target, outputPinNormal);
					}
					catch (Exception e)
					{
						Console.WriteLine("Exception in LoadOutputConnectionDataFromSource():" + e);
					}
					break;
			}
		}

		public void MakeConnection(Node to, FrameworkElement pinFrom)
		{
			Connection c = new Connection(this, pinFrom, to);
			to.allConnections.Add(c);
			allConnections.Add(c);
			outputConnections.Add(c);
			(Parent as Canvas)?.Children.Add(c);
		}

		public void DeleteAllOutputConnections()
		{
			for (int i = outputConnections.Count - 1; i >= 0; i--)
			{
				Connection c = outputConnections[i];
				(c.objTo.Parent as Node)?.allConnections.Remove(c);
				(c.Parent as Canvas)?.Children.Remove(c);
				allConnections.Remove(c);
				outputConnections.Remove(c);
			}
		}


		#endregion

		#region Translation and position

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

		#endregion

		#region Drag'n'Drop node

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			base.OnMouseDown(e);
			//Console.WriteLine("Down");

			if(false == MainWindow.instance.selection.Contains(this))
			{
				MainWindow.instance.ClearSelection();
				MainWindow.instance.selection.Add(this);
			}

			MainWindow.instance.StartDragnDropSelected((Vector)e.GetPosition((IInputElement)Parent));
			CaptureMouse();
			MouseMove += MainWindow.instance.DragnDropSelectedOnMove;
		}


		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			base.OnMouseUp(e);
			//Console.WriteLine("Up");
			ReleaseMouseCapture();
			MouseMove -= MainWindow.instance.DragnDropSelectedOnMove;
		}

		public void ForceConnectionUpdate()
		{
			foreach (var connection in allConnections)
			{
				connection.InvalidateVisual();
			}
		}

		#endregion

		#region Interaction

		public void SetSelected(bool selected)
		{
			selectionBorder.Background = selected ? new SolidColorBrush(Color.FromRgb(224, 224, 128)) : null;
			Console.WriteLine("selected node: " + nodeNameField.Text);
		}

		private void ButtonDelete_Click(object sender, RoutedEventArgs e)
		{
			Delete();
		}

		public void Delete()
		{
			Console.WriteLine("Deleting node: " + nodeNameField.Text);
			MainWindow.instance.DeleteNode(this);
		}

		private void outputType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			DeleteAllOutputConnections();
			//TODO: Change Data in switch below, not only color
			Brush b;
			Color c = new Color();
			c.A = 255;
			switch (outputType.Text)
			{
				
				case "End dialogue":
					c.R = 128; c.G = 0; c.B = 0; //red-ish
					b = new SolidColorBrush(c);
					break;
				case "Multiple choices":
					c.R = 0; c.G = 128; c.B = 64; //green-ish
					b = new SolidColorBrush(c);
					break;
				case "If player has item":
					c.R = 198; c.G = 198; c.B = 47; //yellow-ish
					b = new SolidColorBrush(c);
					break;
				case "Call actor event":
					c.R = 47; c.G = 65; c.B = 198; //blue-ish
					b = new SolidColorBrush(c);
					break;
				case "Call level event":
					c.R = 0; c.G = 10; c.B = 91; //violet-ish
					b = new SolidColorBrush(c);
					break;
				case "Normal dialogue":
				default:
					c.R = 0x3f; c.G = 0x3f; c.B = 0x3f; //gray-ish
					b = new SolidColorBrush(c);
					break;
			}

			BorderUp.Background = BorderMiddle.Background = BorderDown.Background = b;
			//TODO: Fix changing color manually. It doesn't work for some reason.
		}

		private void dialogueText_TextChanged(object sender, TextChangedEventArgs e)
		{

		}


		#endregion
	}
}
