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
				try
				{
					Node n = nodes[item];
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
			for (int i = 0; i < s.Length; i++) 
			{
				try
				{
					Node n = nodes[s[i]];
					if (n.sourceData.prompt == b.Content.ToString()) 
					{
						SetNextDataRow(s[i]);
						Next();
						return;
					}
				}
				catch (Exception)
				{
				}
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

		private void SetNextDataRow(string from)
		{
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
			}
		}

		#endregion

	}
}
