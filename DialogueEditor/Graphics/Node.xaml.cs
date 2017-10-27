using System;
using System.Linq;
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
		
		public List<Connection> inputConnections = new List<Connection>();
		public List<Connection> outputConnections = new List<Connection>();

		public Node(DialogueDataLine sourceData)
		{
			InitializeComponent();
			Width = grid.Width;
			Height = grid.Height;
			this.sourceData = sourceData;
			LoadDataFromSource();
			RecalculatePromptAreaVisibility();
		}

		#region Source data

		public void LoadDataFromSource(bool withoutOutputType = false)
		{
			nodeNameField.Text = sourceData.rowName;
			PromptActorsCombobox.Text = sourceData.boundToActor;
			string[] cmndArgs = sourceData.commandArguments.Split(new string[] { ": " }, StringSplitOptions.None);
			if (cmndArgs.Length == 1) 
			{
				actorName.Text = "None";
				dialogueText.Text = cmndArgs[0];
			}
			else try
			{
				actorName.Text = cmndArgs[0];
				dialogueText.Text = "";
				for (int i = 1; i < cmndArgs.Length - 1; i++)
				{
					dialogueText.Text += cmndArgs[i];
					dialogueText.Text += ": ";
				}
				if (cmndArgs.Length > 1)
				{
					dialogueText.Text += cmndArgs[cmndArgs.Length - 1];
				}
			}
			catch (Exception e)
			{
					Console.WriteLine(e.Message);
			}
			PromptTextBox.Text = sourceData.prompt;
			SetPosition(sourceData.nodePositionX, sourceData.nodePositionY);
			switch (sourceData.command)
			{
				case "leave":
					if (!withoutOutputType) 
					{
						outputType.Text = "End dialogue";
					}
					break;
				case "options":
					if (!withoutOutputType)
					{
						outputType.Text = "Multiple choices";
					}
					break;
				case "has-item":
					if (!withoutOutputType)
					{
						outputType.Text = "If player has item";
					}
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
					if (!withoutOutputType)
					{
						outputType.Text = "Call actor event";
					}
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
					if (!withoutOutputType)
					{
						outputType.Text = "Call level event";
					}
					levelEventName.Text = sourceData.commandArguments;
					break;
				case "dialogue":
					if (!withoutOutputType)
					{
						outputType.Text = "Normal dialogue";
					}
					break;
				case "go-to":
					if (!withoutOutputType)
					{
						outputType.Text = "Shortcut";
					}
					TargetDialogueID.Text = sourceData.nextRowName;
					break;
				case "go-to-target":
					if(!withoutOutputType)
					{
						outputType.Text = "Shortcut target";
					}
					break;
				case "set-bool":
					if(!withoutOutputType)
					{
						outputType.Text = "Set bool";
					}
					string[] __s = sourceData.commandArguments.Split(' ');
					bool _checked = false;
					try
					{
						SetBoolID.Text = __s[0];
						_checked = __s[1] == "true";
					}
					catch (Exception)
					{
					}
					SetBoolValue.IsChecked = _checked;
					break;
				case "get-bool":
					if(!withoutOutputType)
					{
						outputType.Text = "Check bool";
					}
					CheckBoolID.Text = sourceData.commandArguments;
					break;
				default:
					if (!withoutOutputType)
					{
						outputType.Text = "Normal dialogue";
					}
					break;
			}
		}

		public void ApplyChangesToSourceData()
		{
			MainWindow.instance.nodeMap.Remove(sourceData.rowName);

			sourceData.rowName = nodeNameField.Text.ToString();
			sourceData.prompt = PromptTextBox.Text;
			sourceData.boundToActor = PromptActorsCombobox.Text;

			MainWindow.instance.nodeMap.Add(sourceData.rowName, this);

			switch (outputType.Text)
			{
				case "End dialogue":
					sourceData.command = "leave";
					sourceData.commandArguments = string.Empty;
					break;
				case "Shortcut target":
					sourceData.command = "go-to-target";
					sourceData.commandArguments = string.Empty;
					break;
				case "Shortcut":
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
					sourceData.commandArguments = actorName.Text + ": " + dialogueText.Text;
					break;
				case "Check bool":
					sourceData.command = "get-bool";
					sourceData.commandArguments = CheckBoolID.Text;
					break;
				case "Set bool":
					sourceData.command = "set-bool";
					sourceData.commandArguments = SetBoolID.Text + " " + (SetBoolValue.IsChecked == true ? "true" : "false");
					break;
				default:
					break;
			}
		}

		public DialogueDataLine ToDialogueDataLine()
		{
			DialogueDataLine d = new DialogueDataLine();

			d.rowName = nodeNameField.Text;
			d.prompt = PromptTextBox.Text;
			d.boundToActor = PromptActorsCombobox.Text;
			Point p = GetPosition();
			d.SetPosition(p.X, p.Y);

			switch (outputType.Text)
			{
				case "End dialogue":
					d.command = "leave";
					d.commandArguments = string.Empty;
					break;
				case "Shortcut target":
					d.command = "go-to-target";
					d.commandArguments = string.Empty;
					break;
				case "Shortcut":
					d.command = "go-to";
					d.commandArguments = string.Empty;
					d.nextRowName = TargetDialogueID.Text;
					break;
				case "Multiple choices":
					d.command = "options";
					d.commandArguments = string.Empty;
					break;
				case "If player has item":
					d.command = "has-item";
					d.commandArguments = itemName.Text + " " + itemCount.Text;
					break;
				case "Call actor event":
					d.command = "actor-message";
					d.commandArguments = eventActorName.Text + " " + eventActorEventName.Text;
					break;
				case "Call level event":
					d.command = "level-message";
					d.commandArguments = levelEventName.Text;
					break;
				case "Normal dialogue":
					d.command = "dialogue";
					d.commandArguments = actorName.Text + ": " + dialogueText.Text;
					break;
				case "Check bool":
					d.command = "get-bool";
					d.commandArguments = CheckBoolID.Text;
					break;
				case "Set bool":
					d.command = "set-bool";
					d.commandArguments = SetBoolID.Text + " " + (SetBoolValue.IsChecked == true ? "true" : "false");
					break;
				default:
					break;
			}

			return d;
		}

		public void ApplyConnectionChangesToSourceData()
		{
			switch (outputType.Text)
			{
				case "End dialogue":
					sourceData.nextRowName = "None";
					break;
				case "Shortcut":
					sourceData.nextRowName = TargetDialogueID.Text;
					break;
				case "Multiple choices":
					{
						System.Text.StringBuilder s = new System.Text.StringBuilder();
						Connection defaultC = null;
						foreach (var item in outputConnections)
						{
							if (item.objFrom == outputPinMultipleChoices)
							{
								s.Append(item.parentTo.sourceData.rowName);
								s.Append(' ');
							}
							else if (item.objFrom == outputPinMultipleChoicesDefault)
							{
								defaultC = item;
							}
						}
						if (s.Length > 0)
						{
							s.Length--;
						}
						sourceData.nextRowName = s.ToString();

						if (defaultC != null)
						{
							sourceData.commandArguments = SecondsTextBox.Text + " " + defaultC.parentTo.sourceData.rowName;
						}
						else
						{
							sourceData.commandArguments = string.Empty;
						}
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
				case "Check bool":
					{
						string sTrue = "None";
						string sFalse = "None";

						foreach (var item in outputConnections)
						{
							if (item.objFrom == outputPinCheckBoolTrue) 
							{
								sTrue = item.parentTo.sourceData.rowName;
							}
							else if(item.objFrom == outputPinCheckBoolFalse)
							{
								sFalse = item.parentTo.sourceData.rowName;
							}
						}
						sourceData.nextRowName = sTrue + " " + sFalse;
					}
					break;
				case "Shortcut target":
				case "Call actor event":
				case "Call level event":
				case "Normal dialogue":
				case "Set bool":
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

		#endregion

		#region Connections

		// 		public FrameworkElement[] GetActiveOutputPins()
		// 		{
		// 			switch (outputType.Text)
		// 			{
		// 
		// 				case "End dialogue":
		// 					return null;
		// 				case "Multiple choices":
		// 					return new FrameworkElement[] { outputPinMultipleChoices, outputPinMultipleChoicesDefault };
		// 				case "If player has item":
		// 					return new FrameworkElement[] { outputPinItemTrue, outputPinItemFalse };
		// 				case "Call actor event":
		// 					return new FrameworkElement[] { outputPinActorEvent };
		// 				case "Call level event":
		// 					return new FrameworkElement[] { outputPinLevelEvent };
		// 				case "Normal dialogue":
		// 				default:
		// 					return new FrameworkElement[] { outputPinNormal };
		// 			}
		// 		}

		private void OnPinMousedDown(object sender, RoutedEventArgs e)
		{
			if(Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
			{
				if (sender == InputPin) 
				{
					DeleteAllInputConnections();
				}
				else
				{
					DeleteAllOutputConnectionsFromPin((FrameworkElement)sender);
				}
			}
			else
			{
				FrameworkElement pin = sender as FrameworkElement;
				MainWindow.instance.StartDrawingConnection(this, pin);
				pin.CaptureMouse();
				pin.MouseMove += MainWindow.instance.ConnnectionDrawingOnMouseMoved;
				pin.PreviewMouseDown -= OnPinMousedDown;
				pin.PreviewMouseUp += OnPinMousedUp;
				e.Handled = true;
			}
		}

		private void OnPinMousedUp(object sender, RoutedEventArgs e)
		{
			FrameworkElement pin = sender as FrameworkElement;
			pin.ReleaseMouseCapture();
			pin.MouseMove -= MainWindow.instance.ConnnectionDrawingOnMouseMoved;
			MainWindow.instance.EndDrawingConnection();
			pin.PreviewMouseUp -= OnPinMousedUp;
			pin.PreviewMouseDown += OnPinMousedDown;
			e.Handled = true;
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

						if (sourceData.commandArguments.Length > 0) 
						{
							try
							{
								s = sourceData.commandArguments.Split(' ');
								SecondsTextBox.Text = s[0];
								Node target = MainWindow.instance.nodeMap[s[1]];
								MakeConnection(target, outputPinMultipleChoicesDefault);
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
				case "set-bool":
					try
					{
						Node target = MainWindow.instance.nodeMap[sourceData.nextRowName];
						MakeConnection(target, outputPinSetBool);
					}
					catch (Exception e)
					{
						Console.WriteLine("Exception in LoadOutputConnectionDataFromSource():" + e);
					}
					break;
				case "get-bool":
					string[] _s = sourceData.nextRowName.Split(' ');
					try
					{
						Node target = MainWindow.instance.nodeMap[_s[0]];
						MakeConnection(target, outputPinCheckBoolTrue);
					}
					catch (Exception e)
					{
						Console.WriteLine("Exception in LoadOutputConnectionDataFromSource():" + e);
					}
					try
					{
						Node target = MainWindow.instance.nodeMap[_s[1]];
						MakeConnection(target, outputPinCheckBoolFalse);
					}
					catch (Exception e)
					{
						Console.WriteLine("Exception in LoadOutputConnectionDataFromSource():" + e);
					}
					break;
			}
		}

		public Connection MakeConnection(Node to, FrameworkElement pinFrom)
		{
			Connection c = new Connection(this, pinFrom, to);
			to.inputConnections.Add(c);
			outputConnections.Add(c);
			(Parent as Canvas)?.Children.Add(c);
			
			to.RecalculatePromptAreaVisibility();

			return c;
		}

		public void DeleteAllOutputConnections()
		{
			// 			for (int i = outputConnections.Count - 1; i >= 0; i--)
			// 			{
			// 				Connection c = outputConnections[i];
			// 				c.parentTo.inputConnections.Remove(c);
			// 				(c.Parent as Canvas)?.Children.Remove(c);
			// 				outputConnections.Remove(c);
			// 
			// 				c.parentTo.RecalculatePromptAreaVisibility();
			// 			}
			if (outputConnections.Count > 0) 
			{
				History.History.Do(new History.Actions.Action_ConnectionsRemoved(outputConnections.ToArray()));
			}
		}

		public void DeleteAllInputConnections()
		{
			// 			for (int i = inputConnections.Count - 1; i >= 0; i--)
			// 			{
			// 				Connection c = inputConnections[i];
			// 				c.parentFrom.outputConnections.Remove(c);
			// 				(c.Parent as Canvas)?.Children.Remove(c);
			// 				inputConnections.Remove(c);
			// 			}
			// 
			// 			RecalculatePromptAreaVisibility();
			if (inputConnections.Count > 0) 
			{
				History.History.Do(new History.Actions.Action_ConnectionsRemoved(inputConnections.ToArray()));
			}
		}

		public void DeleteAllConnections()
		{
			if (inputConnections.Count > 0 || outputConnections.Count > 0)
			{
				History.History.Do(new History.Actions.Action_ConnectionsRemoved(inputConnections.Concat(outputConnections).ToArray()));
			}

			// 			DeleteAllInputConnections();
			// 			DeleteAllOutputConnections();
		}

		public bool PinHasConnection(FrameworkElement pin)
		{
			if (pin == InputPin) 
			{
				return inputConnections.Count > 0;		
			}
			else
			{
				foreach (var c in outputConnections) 
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
			List<Connection> connectionsToDelete = new List<Connection>();

			foreach (var c in outputConnections) 
			{
				if (c.objFrom == pin) 
				{
					connectionsToDelete.Add(c);
				}
			}

			History.History.Do(new History.Actions.Action_ConnectionsRemoved(connectionsToDelete.ToArray()));
		}

		public void TryConnecting(FrameworkElement thisPin, Node other, FrameworkElement otherPin)
		{
			if (thisPin == InputPin) 
			{
				switch (other.outputType.Text)
				{
					case "End dialogue":
						return;
					case "Shortcut":
						History.History.Do(new History.Actions.Action_ConnectionMadeGoTo(other, this));
						other.PromptTextBox.Text = this.PromptTextBox.Text;
						//other.TargetDialogueID.Text = sourceData.rowName;
						break;
					case "Multiple choices":
						if(otherPin == other.outputPinMultipleChoicesDefault)
						{
							if(other.PinHasConnection(otherPin))
							{
								other.DeleteAllOutputConnectionsFromPin(otherPin);
							}
						}
						History.History.Do(new History.Actions.Action_ConnectionMade(other, otherPin, this));
						//other.MakeConnection(this, otherPin);
						break;
					case "Check bool":
					case "Shortcut target":
					case "Call actor event":
					case "Call level event":
					case "Normal dialogue":
					case "Set bool":
					case "If player has item":
						if(other.PinHasConnection(otherPin))
						{
							other.DeleteAllOutputConnectionsFromPin(otherPin);
						}
						History.History.Do(new History.Actions.Action_ConnectionMade(other, otherPin, this));
						//other.MakeConnection(this, otherPin);
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
					case "Shortcut":
						TryConnecting(thisPin, other, null);
						//other.TargetDialogueID.Text = sourceData.rowName;
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
					case "Check bool":
						if(other.PinHasConnection(other.outputPinCheckBoolTrue))
						{
							TryConnecting(thisPin, other, other.outputPinCheckBoolFalse);
						}
						else
						{
							TryConnecting(thisPin, other, other.outputPinCheckBoolTrue);
						}
						break;
					case "Set bool":
						TryConnecting(thisPin, other, other.outputPinSetBool);
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

			if (e.LeftButton != MouseButtonState.Pressed) 
			{
				return;
			}

			if(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
			{
				if (false == MainWindow.instance.selection.Contains(this))
				{
					MainWindow.instance.selection.Add(this);
					SetSelected(true);
				}
			}
			else if(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
			{
				MainWindow.instance.selection.Remove(this);
				SetSelected(false);
				return;
			}
			else
			{
				if (false == MainWindow.instance.selection.Contains(this))
				{
					MainWindow.instance.ClearSelection();
					MainWindow.instance.selection.Add(this);
					SetSelected(true);
				}
				
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
			//Console.WriteLine("selected node: " + nodeNameField.Text);
		}

		private void outputType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			DeleteAllOutputConnections();
			//TODO: Change Data in switch below, not only color
			Color c = new Color();
			c.A = 255;

			string s;
			try
			{
				s = e.AddedItems[0].ToString().Substring("System.Windows.Controls.ComboBoxItem: ".Length);
			}
			catch (Exception)
			{
				return;
			}

			switch (s)
			{
				case "End dialogue":
					c.R = 128; c.G = 0; c.B = 0; //red
					break;
				case "Shortcut":
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
					c.R = 114; c.G = 69; c.B = 114; //violet
					break;
				case "Normal dialogue":
				default:
					c.R = 98; c.G = 110; c.B = 130; //gray
					break;
				case "Shortcut target":
					c.R = 232; c.G = 225; c.B = 46;	//bright yellow
					break;
				case "Check bool":
					c.R = 0x42; c.G = 02; c.B = 02;	//red-ish
					break;
				case "Set bool":
					c.R = 0xFF; c.G = 0x4E; c.B = 0x4E; //red-ish
					break;
			}

			BorderUp.Background = BorderMiddle.Background = BorderDown.Background = new SolidColorBrush(c);

			if (s != outputType.Text)
			{
				History.History.AddToUndoHistory(new History.Actions.Action_NodeTypeChanged(this, outputType.Text, s));
			}
		}

		private void nodeNameField_TextChanged(object sender, TextChangedEventArgs e)
		{
			nodeNameField.Text = nodeNameField.Text.ToString().Replace(" ", "");
			// 			if (MainWindow.instance.nodeMap.ContainsKey(nodeNameField.Text))
			// 			{
			// 				MessageBox.Show("Node names have to be unique!");
			// 				nodeNameField.Text = Guid.NewGuid().ToString();
			// 			}
			if (sourceData != null)
			{
				string oldName = sourceData.rowName;
				MainWindow.instance.nodeMap.Remove(oldName);
				MainWindow.instance.nodeMap.Add(nodeNameField.Text.ToString(), this);
			}

		}

		private void RemoveSpacesFromSender(object sender, TextChangedEventArgs e)
		{
			TextBox b = sender as TextBox;
			b.Text = b.Text.Replace(" ", "");
		}

		public bool IsFlowControlNode
		{
			get
			{
				return outputType.Text == "Check bool" || outputType.Text == "If player has item";
			}
		}

		public bool RecalculatePromptAreaVisibility()
		{
			if(IsFlowControlNode)
			{
				BorderPrompt.Visibility = Visibility.Collapsed;

				foreach(var item in inputConnections)
				{
					if (item.parentFrom.outputType.Text == "Multiple choices")
					{
						if (!(item.parentFrom.outputPinMultipleChoicesDefault == item.objFrom))
						{
							return true;
						}
					}
					else if (item.parentFrom.IsFlowControlNode)
					{
						if (item.parentFrom.RecalculatePromptAreaVisibility())
						{
							return true;
						}
					}
				}

				return false;
			}

			foreach (var item in inputConnections)
			{
				if (item.parentFrom.outputType.Text == "Multiple choices")
				{
					if (!(item.parentFrom.outputPinMultipleChoicesDefault == item.objFrom))
					{
						BorderPrompt.Visibility = Visibility.Visible;
						return true;
					}
				}
				else if (item.parentFrom.IsFlowControlNode) 
				{
					if(item.parentFrom.RecalculatePromptAreaVisibility())
					{
						BorderPrompt.Visibility = Visibility.Visible;
						return true;
					}
				}
			}
			BorderPrompt.Visibility = Visibility.Collapsed;
			return false;
		}

		public void CreateUniqueID()
		{
			nodeNameField.Text = actorName.Text + Guid.NewGuid().ToString();
		}

		private void HighlightPinConnections(object sender, MouseEventArgs e)
		{
			if (sender == InputPin) 
			{
				foreach (var item in inputConnections)
				{
					item.SetHighlightEnabled(true);
				}
			}
			else
			{
				foreach (var item in outputConnections)
				{
					if(item.objFrom == sender)
					{
						item.SetHighlightEnabled(true);
					}
				}
			}
		}

		private void UnhighlightPinConnections(object sender, MouseEventArgs e)
		{
			if (sender == InputPin)
			{
				foreach (var item in inputConnections)
				{
					item.SetHighlightEnabled(false);
				}
			}
			else
			{
				foreach (var item in outputConnections)
				{
					if (item.objFrom == sender)
					{
						item.SetHighlightEnabled(false);
					}
				}
			}
		}

		private void OnNodeDataChanged(object sender, RoutedEventArgs e)
		{
			DialogueDataLine oldData = sourceData;
			DialogueDataLine newData = ToDialogueDataLine();

			History.History.Do(new History.Actions.Action_NodeDataChanged(this, oldData, newData));
		}


		private void PromptActorsCombobox_LostFocus(object sender, RoutedEventArgs e)
		{
			try
			{
				if (!AdditionalData.GetWidgetAnchorNames().Contains(PromptActorsCombobox.Text) && PromptActorsCombobox.Text.Length > 0)
				{
					AdditionalData.AddWidgetAnchor(PromptActorsCombobox.Text);
				}
			}
			catch (Exception)
			{
				
			}
			OnNodeDataChanged(sender, e);
		}

		private void PromptActorsCombobox_GotFocus(object sender, RoutedEventArgs e)
		{
			PromptActorsCombobox.ItemsSource = AdditionalData.GetWidgetAnchorNames();
		}

		private void actorName_GotFocus(object sender, RoutedEventArgs e)
		{
			actorName.ItemsSource = AdditionalData.GetActorNames();
		}

		private void actorName_LostFocus(object sender, RoutedEventArgs e)
		{
			try { 
				if (!AdditionalData.GetActorNames().Contains(actorName.Text) && actorName.Text.Length > 0 && actorName.Text != "None")
				{
					AdditionalData.AddDialogueActor(actorName.Text);
				}
			}
			catch (Exception)
			{
				
			}
			OnNodeDataChanged(sender, e);
		}

		private void itemName_LostFocus(object sender, RoutedEventArgs e)
		{
			try { 
				if (!AdditionalData.GetItemNames().Contains(itemName.Text) && itemName.Text.Length > 0 && itemName.Text != "None")
				{
					AdditionalData.AddItemName(itemName.Text);
				}
			}
			catch (Exception)
			{
				
			}
			OnNodeDataChanged(sender, e);
		}

		private void itemName_GotFocus(object sender, RoutedEventArgs e)
		{
			itemName.ItemsSource = AdditionalData.GetItemNames();
		}

		private void itemName_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			OnNodeDataChanged(sender, e);
		}

		#endregion

		private void TargetDialogueID_LostFocus(object sender, RoutedEventArgs e)
		{
			try
			{
				Node n = MainWindow.instance.nodeMap[TargetDialogueID.Text];
				PromptTextBox.Text = n.PromptTextBox.Text;
			}
			catch (Exception)
			{
				
			}

			OnNodeDataChanged(sender, e);
		}

		private void DontAllowSpaces(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Space)
			{
				e.Handled = true;
			}
		}
	}
}
