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

		//public List<Connection> allConnections = new List<Connection>();
		public List<Connection> inputConnections = new List<Connection>();
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
			nodeNameField.Content = sourceData.rowName;
			dialogueText.Text = sourceData.commandArguments;
			SetPosition(sourceData.nodePositionX, sourceData.nodePositionY);
			switch (sourceData.command)
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
						eventActorName.Text = s[0];
						eventActorEventName.Text = s[1];
					}
					catch (Exception)
					{
					}
					break;
				case "level-message":
					outputType.Text = "Call level event";
					levelEventName.Text = sourceData.commandArguments;
					break;
				case "dialogue":
					outputType.Text = "Normal dialogue";
					break;
				case "go-to":
					outputType.Text = "Go to node";
					TargetDialogueID.Text = sourceData.nextRowName;
					break;
				default:
					outputType.Text = "Normal dialogue";
					break;
			}
		}



		#region Connections

		public FrameworkElement[] GetActiveOutputPins()
		{
			switch (outputType.Text)
			{

				case "End dialogue":
					return null;
				case "Multiple choices":
					return new FrameworkElement[] { outputPinMultipleChoices };
				case "If player has item":
					return new FrameworkElement[] { outputPinItemTrue, outputPinItemFalse };
				case "Call actor event":
					return new FrameworkElement[] { outputPinActorEvent };
				case "Call level event":
					return new FrameworkElement[] { outputPinLevelEvent };
				case "Normal dialogue":
				default:
					return new FrameworkElement[] { outputPinNormal };
			}
		}

		private void OnPinMousedDown(object sender, RoutedEventArgs e)
		{
			RadioButton pin = sender as RadioButton;
			MainWindow.instance.StartDrawingConnection(this, pin);
			pin.CaptureMouse();
			pin.MouseMove += MainWindow.instance.ConnnectionDrawingOnMouseMoved;
			pin.Click -= OnPinMousedDown;
			pin.Click += OnPinMousedUp;
		}

		private void OnPinMousedUp(object sender, RoutedEventArgs e)
		{
			RadioButton pin = sender as RadioButton;
			pin.ReleaseMouseCapture();
			pin.MouseMove -= MainWindow.instance.ConnnectionDrawingOnMouseMoved;
			MainWindow.instance.EndDrawingConnection();
			pin.MouseUp -= OnPinMousedUp;
			pin.Click -= OnPinMousedUp;
			pin.Click += OnPinMousedDown;
			pin.IsChecked = false;
		}

		public void LoadOutputConnectionDataFromSource()
		{
			switch (sourceData.command)
			{
				case "leave":
					break;
				case "go-to":
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
							Node target = MainWindow.instance.nodeMap[s[0]];
							MakeConnection(target, outputPinItemTrue);
						}
						catch (Exception e)
						{
							Console.WriteLine("Exception in LoadOutputConnectionDataFromSource():" + e);
						}
						try
						{
							Node target = MainWindow.instance.nodeMap[s[1]];
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
				case "dialogue":
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
			to.inputConnections.Add(c);
			outputConnections.Add(c);
			(Parent as Canvas)?.Children.Add(c);
		}

		public void DeleteAllOutputConnections()
		{
			for (int i = outputConnections.Count - 1; i >= 0; i--)
			{
				Connection c = outputConnections[i];
				c.parentTo.inputConnections.Remove(c);
				(c.Parent as Canvas)?.Children.Remove(c);
				outputConnections.Remove(c);
			}
		}

		public void DeleteAllInputConnections()
		{
			for (int i = inputConnections.Count - 1; i >= 0; i--)
			{
				Connection c = inputConnections[i];
				c.parentTo.outputConnections.Remove(c);
				(c.Parent as Canvas)?.Children.Remove(c);
				inputConnections.Remove(c);
			}
		}

		public void DeleteAllConnections()
		{
			DeleteAllInputConnections();
			DeleteAllOutputConnections();
		}

		public bool PinHasConnection(FrameworkElement pin)
		{
			if (pin == InputPin) 
			{
				return inputConnections.Count > 0;		
			}
			else
			{
				foreach(var c in outputConnections)
				{
					if(c.objFrom == pin)
					{
						return true;
					}
				}
			}
			return false;
		}

		private void DeleteAllOutputConnectionsFromPin(FrameworkElement pin)
		{
			for (int i = outputConnections.Count-1; i >= 0; i--)
			{
				Connection c = outputConnections[i];
				if (c.objFrom == pin) 
				{
					c.parentTo.inputConnections.Remove(c);
					(c.Parent as Canvas)?.Children.Remove(c);
					outputConnections.Remove(c);
				}
			}
		}

		public void TryConnecting(FrameworkElement thisPin, Node other, FrameworkElement otherPin)
		{
			if (thisPin == InputPin) 
			{
				switch (other.outputType.Text)
				{

					case "End dialogue":
						return;
					case "Go to node":
						other.TargetDialogueID.Text = sourceData.rowName;
						break;
					case "Multiple choices":
						other.MakeConnection(this, other);
						break;
					case "Call actor event":
					case "Call level event":
					case "Normal dialogue":
					case "If player has item":
						if(other.PinHasConnection(otherPin))
						{
							other.DeleteAllOutputConnectionsFromPin(otherPin);
						}
						other.MakeConnection(this, otherPin);
						break;
					default:
						break;
				}
			}
			else
			{
				//No need to implement the same shit twice, yo
				other.TryConnecting(otherPin, this, thisPin);
			}
		}

		public void TryConnecting(FrameworkElement thisPin, Node other)
		{
			if (thisPin == InputPin)
			{
				switch (other.outputType.Text)
				{
					case "End dialogue":
						Console.WriteLine("Trying to connect input to \"End\" node. Aborting.");
						return;
					case "Go to node":
						other.TargetDialogueID.Text = sourceData.rowName;
						break;
					case "Multiple choices":
							TryConnecting(thisPin, other, other.outputPinMultipleChoices);
						break;
					case "If player has item":
						if(other.PinHasConnection(other.outputPinItemTrue))
						{
							TryConnecting(thisPin, other, other.outputPinItemFalse);
						}
						else
						{
							TryConnecting(thisPin, other, other.outputPinItemTrue);
						}
						break;
					case "Call actor event":
						TryConnecting(thisPin, other, other.outputPinActorEvent);
						break;
					case "Call level event":
						TryConnecting(thisPin, other, other.outputPinLevelEvent);
						break;
					case "Normal dialogue":
					default:
						TryConnecting(thisPin, other, other.outputPinNormal);
						break;
				}
			}
			else
			{
				TryConnecting(thisPin, other, other.InputPin);
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
				SetSelected(true);
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
			MainWindow.instance.EndDragnDropSelected();
		}

		protected void OnMouseControlLost(object sender, MouseEventArgs e)
		{
			OnMouseUp(null);
		}

		public void ForceConnectionUpdate()
		{
			foreach (var connection in inputConnections)
			{
				connection.InvalidateVisual();
			}
			foreach (var connection in outputConnections)
			{
				connection.InvalidateVisual();
			}
		}

		#endregion

		#region Interaction

		public void SetSelected(bool selected)
		{
			selectionBorder.Background = selected ? new SolidColorBrush(Color.FromRgb(224, 224, 128)) : null;
			Console.WriteLine("selected node: " + nodeNameField.Content);
		}

		private void ButtonDelete_Click(object sender, RoutedEventArgs e)
		{
			Delete();
		}

		public void Delete()
		{
			Console.WriteLine("Deleting node: " + nodeNameField.Content);
			MainWindow.instance.DeleteNode(this);
		}

		private void outputType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			DeleteAllOutputConnections();
			//TODO: Change Data in switch below, not only color
			Color c = new Color();
			c.A = 255;

			string s = e.AddedItems[0].ToString().Substring("System.Windows.Controls.ComboBoxItem: ".Length);

			switch (s)
			{
				case "End dialogue":
					c.R = 128; c.G = 0; c.B = 0; //red
					break;
				case "Go to node":
					c.R = 196; c.G = 153; c.B = 0;	//orange
					break;
				case "Multiple choices":
					c.R = 0; c.G = 128; c.B = 64; //green
					break;
				case "If player has item":
					c.R = 198; c.G = 198; c.B = 47; //yellow
					break;
				case "Call actor event":
					c.R = 47; c.G = 65; c.B = 198; //blue
					break;
				case "Call level event":
					c.R = 0; c.G = 10; c.B = 91; //violet
					break;
				case "Normal dialogue":
				default:
					c.R = 0x3f; c.G = 0x3f; c.B = 0x3f; //gray
					break;
			}

			BorderUp.Background = BorderMiddle.Background = BorderDown.Background = new SolidColorBrush(c);
		}

		private void nodeNameField_TextChanged(object sender, TextChangedEventArgs e)
		{
			nodeNameField.Content = nodeNameField.Content.ToString().Replace(" ", "");
			// 			if (MainWindow.instance.nodeMap.ContainsKey(nodeNameField.Text))
			// 			{
			// 				MessageBox.Show("Node names have to be unique!");
			// 				nodeNameField.Text = Guid.NewGuid().ToString();
			// 			}
			if (sourceData != null)
			{
				string oldName = sourceData.rowName;
				MainWindow.instance.nodeMap.Remove(oldName);
				MainWindow.instance.nodeMap.Add(nodeNameField.Content.ToString(), this);
			}

		}

		private void RemoveSpacesFromSender(object sender, TextChangedEventArgs e)
		{
			TextBox b = sender as TextBox;
			b.Text = b.Text.Replace(" ", "");
		}

		#endregion

		public void ApplyChangesToSourceData()
		{
			sourceData.rowName = nodeNameField.Content.ToString();

			switch (outputType.Text)
			{
				case "End dialogue":
					sourceData.command = "leave";
					sourceData.commandArguments = string.Empty;
					break;
				case "":
					sourceData.command = "go-to";
					sourceData.commandArguments = string.Empty;
					break;
				case "Multiple choices":
					sourceData.command = "options";
					sourceData.commandArguments = string.Empty;
					break;
				case "If player has item":
					sourceData.command = "has-item";
					sourceData.commandArguments = itemName.Text + " " + itemCount.Text;
					break;
				case "Call actor event":
					sourceData.command = "actor-message";
					sourceData.commandArguments = eventActorName.Text + " " + eventActorEventName.Text;
					break;
				case "Call level event":
					sourceData.command = "level-message";
					sourceData.commandArguments = levelEventName.Text;
					break;
				case "Normal dialogue":
					sourceData.command = "dialogue";
					sourceData.commandArguments = dialogueText.Text;
					break;
				default:
					break;
			}
		}

		public void ApplyConnectionChangesToSourceData()
		{
			switch (outputType.Text)
			{
				case "End dialogue":
					sourceData.nextRowName = "None";
					break;
				case "Go to node":
					sourceData.nextRowName = TargetDialogueID.Text;
					break;
				case "Multiple choices":
					{
						System.Text.StringBuilder s = new System.Text.StringBuilder();
						foreach (var item in outputConnections)
						{
							s.Append(item.parentTo.sourceData.rowName);
							s.Append(' ');
						}
						if (s.Length > 0) 
						{
							s.Length--;
						} 						
						sourceData.nextRowName = s.ToString();
					}
					break;
				case "If player has item":
					{
						string sTrue = "None";
						string sFalse = "None";

						foreach (var item in outputConnections)
						{
							if (item.objFrom == outputPinItemTrue)
							{
								sTrue = item.parentTo.sourceData.rowName;
							}
							else if (item.objFrom == outputPinItemFalse) 
							{
								sFalse = item.parentTo.sourceData.rowName;
							}
						}
						sourceData.nextRowName = sTrue + " " + sFalse;
					}
					break;
				case "Call actor event":
				case "Call level event":
				case "Normal dialogue":
				default:
					if (outputConnections.Count > 0) 
					{
						sourceData.nextRowName = outputConnections[0].parentTo.sourceData.rowName;
					}
					else
					{
						sourceData.nextRowName = "None";
					}
					break;
			}
		}

		public void CreateUniqueID()
		{
			nodeNameField.Content = actorName.Text + Guid.NewGuid().ToString();
		}

		private void aactorName_TextChanged(object sender, TextChangedEventArgs e)
		{
			CreateUniqueID();
		}

		private void dialogueText_TextChanged(object sender, TextChangedEventArgs e)
		{
			CreateUniqueID();
		}
	}
}
