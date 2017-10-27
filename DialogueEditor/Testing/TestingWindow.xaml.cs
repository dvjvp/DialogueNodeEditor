using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DialogueEditor.Testing
{
	/// <summary>
	/// Interaction logic for TestingWindow.xaml
	/// </summary>
	public partial class TestingWindow : Window
	{
		Dictionary<string, Node> nodes;
		Dictionary<string, bool> dialogueFlags = new Dictionary<string, bool>();
		Node currentNode;

		Node errorNode;
		Node errorNode2;

		Thickness margins = new Thickness(5);


		public TestingWindow(Dictionary<string, Node> nodes, Node first)
		{
			InitializeComponent();

			this.nodes = nodes;
			currentNode = first;
			Next();

			errorNode = new Node(new Files.DialogueDataLine("errorNode", "", "dialogue", "Error: couldn't find next row. Aborting.", "errorNode2", ""));
			errorNode2 = new Node(new Files.DialogueDataLine("errorNode2", "", "leave", "", "None", ""));
			try
			{
				this.nodes.Add("errorNode2", errorNode2);
			}
			catch (Exception)
			{
			}
		}

		#region Button creating and deleting

		public void DeleteAllButtons()
		{
			Buttons.Children.Clear();
		}

		private Button AddNextButton()
		{
			var button = new Button();
			button.Content = "(Continue)";
			Buttons.Children.Add(button);
			button.Margin = margins;
			button.Click += GoToNext;
			Buttons.Columns = Buttons.Children.Count;
			return button;
		}
		private Button AddCloseWindowButton()
		{
			var button = new Button();
			button.Content = "(Close this window)";
			Buttons.Children.Add(button);
			button.Margin = margins;
			button.Click += CloseThisWindow;
			Buttons.Columns = Buttons.Children.Count;
			return button;
		}
		private void AddCheckForItemButtons()
		{
			var buttonTrue = new Button();
			buttonTrue.Content = "(Player has item)";
			Buttons.Children.Add(buttonTrue);
			buttonTrue.Margin = margins;
			buttonTrue.Click += ButtonTrue_Click;

			var buttonFalse = new Button();
			buttonFalse.Content = "(Player doesn't have item)";
			Buttons.Children.Add(buttonFalse);
			buttonFalse.Margin = margins;
			Buttons.Columns = Buttons.Children.Count;
			buttonFalse.Click += ButtonFalse_Click;
		}
		private void AddOptionsButtons()
		{
			string[] s = currentNode.sourceData.nextRowName.Split(' ');
			foreach (var item in s)
			{
				string actualItem = ResolveNodeID(item);
				if (actualItem == "None") continue;

				try
				{
					Node n = nodes[actualItem];
					var button = new Button();
					button.Content = n.sourceData.prompt;
					Buttons.Children.Add(button);
					button.Margin = margins;
					button.Click += OnOptionSelected;
				}
				catch (Exception)
				{
				}
			}

			if (currentNode.sourceData.commandArguments.Length > 0) 
			{
				string[] s2 = currentNode.sourceData.commandArguments.Split(' ');
				var button = new Button();
				button.Content = "<<Wait \"" + s2[0] + "\" seconds instead.>>";
				Buttons.Children.Add(button);
				button.Margin = margins;
				button.Click += WaitedThroughOptionSelection;
			}
			Buttons.Columns = Buttons.Children.Count;
		}

		private void AddCheckBoolButtons(bool defaultValue)
		{
			var buttonDefault = new Button();
			buttonDefault.Content = defaultValue ? "Default (true)" : "Default (false)";
			Buttons.Children.Add(buttonDefault);
			buttonDefault.Margin = margins;
			if (defaultValue)
			{
				buttonDefault.Click += ButtonTrue_Click;
			}
			else
			{
				buttonDefault.Click += ButtonFalse_Click;
			}

			var buttonTrue = new Button();
			buttonTrue.Content = "Force TRUE";
			Buttons.Children.Add(buttonTrue);
			buttonTrue.Margin = margins;
			buttonTrue.Click += ButtonTrue_Click;

			var buttonFalse = new Button();
			buttonFalse.Content = "Force FALSE";
			Buttons.Children.Add(buttonFalse);
			buttonFalse.Margin = margins;
			Buttons.Columns = Buttons.Children.Count;
			buttonFalse.Click += ButtonFalse_Click;
		}


		#endregion

		#region On button(s) clicked

		private void WaitedThroughOptionSelection(object sender, RoutedEventArgs e)
		{
			string[] s = currentNode.sourceData.commandArguments.Split(' ');
			SetNextDataRow(s[1]);
			Next();
		}

		private void OnOptionSelected(object sender, RoutedEventArgs e)
		{
			Button b = sender as Button;
			string[] s = currentNode.sourceData.nextRowName.Split(' ');
			int index = Buttons.Children.IndexOf(b);

			try
			{
				Node n = nodes[s[index]];
				SetNextDataRow(s[index]);
				Next();
				return;
			}
			catch (Exception)
			{
			}
		}

		private void ButtonFalse_Click(object sender, RoutedEventArgs e)
		{
			string[] s = currentNode.sourceData.nextRowName.Split(' ');
			SetNextDataRow(s[1]);
			Next();
		}
		private void ButtonTrue_Click(object sender, RoutedEventArgs e)
		{
			string[] s = currentNode.sourceData.nextRowName.Split(' ');
			SetNextDataRow(s[0]);
			Next();
		}

		private void CloseThisWindow(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void GoToNext(object sender, RoutedEventArgs e)
		{
			Next();
		}

		#endregion

		#region Dialogue System Interpreter

		private string ResolveNodeID(string inID)
		{
			if(inID == "None")
			{
				return "None";
			}

			Node node;
			try
			{
				node = nodes[inID];
			}
			catch (Exception)
			{
				node = errorNode;
			}

			if(node.sourceData.command == "get-bool")
			{
				string nextName = "None";
				string boolName = node.sourceData.commandArguments;
				string[] n = node.sourceData.nextRowName.Split(' ');

				if (!dialogueFlags.ContainsKey(boolName)) 
				{
					dialogueFlags.Add(boolName, false);
				}
				if (dialogueFlags[boolName]) 
				{
					try
					{
						nextName = n[0];
					}
					catch (Exception)
					{ }
				}
				else
				{
					try
					{
						nextName = n[1];
					}
					catch (Exception)
					{ }
				}

				return ResolveNodeID(nextName);

			}
			else if(node.sourceData.command == "has-item")
			{
				string message = "Does player have at least ";

				string[] s = node.sourceData.commandArguments.Split(' ');
				string count = "-1";
				string itemName = "<errorName>";
				try
				{
					itemName = s[0];
				}
				catch (Exception)
				{}
				try
				{
					count = s[1];
				}
				catch (Exception)
				{}
				message += count + " " + itemName + "?";

				string nextName = "None";
				string[] n = node.sourceData.nextRowName.Split(' ');

				if (MessageBox.Show(message, "(Resolve has-item sub-tree)", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
				{
					try
					{
						nextName = n[0];
					}
					catch (Exception)
					{}
				}
				else
				{
					try
					{
						nextName = n[1];
					}
					catch (Exception)
					{}
				}

				return ResolveNodeID(nextName);
			}
			else
			{
				return inID;
			}
		}

		private void SetNextDataRow(string from)
		{
			from = ResolveNodeID(from);
			if (from == "None")
			{
				DialogueText.Text = "<<null Node (\"None\") reached. Ending dialogue.>>";
				DeleteAllButtons();
				AddCloseWindowButton();
				return;
			}

			try
			{
				currentNode = nodes[from];
			}
			catch (Exception)
			{
				currentNode = errorNode;
			}
		}

		public void Next()
		{
			DeleteAllButtons();
			switch(currentNode.sourceData.command)
			{
				case "level-message":
					DialogueText.Text = "<<Calling level blueprint event: \"" + currentNode.sourceData.commandArguments + "\">>";
					AddNextButton();
					SetNextDataRow(currentNode.sourceData.nextRowName);
					break;
				case "actor-message":
					DialogueText.Text = "<<Calling actor event: \"" + currentNode.sourceData.commandArguments + "\">>";
					AddNextButton();
					SetNextDataRow(currentNode.sourceData.nextRowName);
					break;
				case "has-item":
					{
						string[] s = currentNode.sourceData.commandArguments.Split(' ');
						DialogueText.Text = "<<Checking whether player has at least \"" + s[1] + "\" items of type \"" + s[0] + "\"...>>";
						AddCheckForItemButtons();
					}
					break;
				case "leave":
					DialogueText.Text = "<<Dialogue ended.>>";
					AddCloseWindowButton();
					break;
				case "go-to":
				case "go-to-target":
					SetNextDataRow(currentNode.sourceData.nextRowName);
					Next();
					break;
				case "dialogue":
					DialogueText.Text = currentNode.sourceData.commandArguments;
					AddNextButton();
					SetNextDataRow(currentNode.sourceData.nextRowName);
					break;
				case "options":
					AddOptionsButtons();
					break;
				case "get-bool":
					{
						string boolName = currentNode.sourceData.commandArguments;
						bool boolValue = false;
						if(dialogueFlags.ContainsKey(boolName))
						{
							boolValue = dialogueFlags[boolName];
						}
						else
						{
							dialogueFlags.Add(boolName, false);
						}

						DialogueText.Text = "<<Checking for bool \"" + boolName + "\", current value: " + (boolValue ? "true" : "false") + "...>>";
						AddCheckBoolButtons(boolValue);
					}
					break;
				case "set-bool":
					{
						string[] s = currentNode.sourceData.commandArguments.Split(' ');
						DialogueText.Text = "<<Setting bool \"" + s[0] + "\" to " + s[1] + "...>>";
						if(dialogueFlags.ContainsKey(s[0]))
						{
							dialogueFlags[s[0]] = (s[1] == "true");
						}
						else
						{
							dialogueFlags.Add(s[0], (s[1] == "true"));
						}
						AddNextButton();
						SetNextDataRow(currentNode.sourceData.nextRowName);
					}
					break;
			}
		}

		#endregion

	}
}
