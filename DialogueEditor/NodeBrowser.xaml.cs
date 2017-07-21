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
using System.Windows.Shapes;

namespace DialogueEditor
{
	/// <summary>
	/// Interaction logic for NodeBrowser.xaml
	/// </summary>
	public partial class NodeBrowser : Window
	{
		[Serializable]
		public class NodeData
		{
			public Node node;
			public string Id { get; set; }
			public string Type { get; set; }
			public string Prompt { get; set; }
			public string BoundToActor { get; set; }
			public string DialogueText { get; set; }
			public string ActorName { get; set; }
			public string EventName { get; set; }
			public string ItemName { get; set; }


			public NodeData(Node from)
			{
				node = from;
				Id = from.nodeNameField.Text;
				Type = from.outputType.Text;
				Prompt = from.PromptTextBox.Text;
				BoundToActor = from.PromptActorsCombobox.Text;
				DialogueText = from.dialogueText.Text;

				ActorName = string.Empty;
				EventName = string.Empty;

				switch(Type)
				{
					case "Normal dialogue":
						ActorName = from.actorName.Text;
						break;
					case "Call actor event":
						ActorName = from.eventActorName.Text;
						EventName = from.eventActorEventName.Text;
						break;
					case "Call level event":
						EventName = from.levelEventName.Text;
						break;
				}

				ItemName = from.itemName.Text;
			}

		}

		public static NodeBrowser instance;

		public List<NodeData> searchResults;

		public NodeBrowser()
		{
			InitializeComponent();
			instance = this;
			Closed += NodeBrowser_Closed;

			LoadNodeListWithFilters();
		}

		private void NodeBrowser_Closed(object sender, EventArgs e)
		{
			instance = null;
		}

		private bool MatchesWithFilters(NodeData node)
		{
			bool typeMatch = (node.Type == Nodetype_Combobox.Text) || (Nodetype_Combobox.Text == "Any");

			return typeMatch
				&& (node.Id.Contains(ID_Textbox.Text))
				&& (node.Prompt.Contains(Prompt_Textbox.Text))
				&& (node.BoundToActor.Contains(BoundToActor_Textbox.Text))
				&& (node.DialogueText.Contains(DialogueText_Textbox.Text))
				&& (node.ActorName.Contains(ActorName_Textbox.Text))
				&& (node.EventName.Contains(EventName_Textbox.Text))
				&& (node.ItemName.Contains(ItemName_Textbox.Text));

		}

		private void LoadNodeListWithFilters()
		{
			searchResults = new List<NodeData>();

			foreach (var node in MainWindow.instance.nodes)
			{
				NodeData n = new NodeData(node);
				if(MatchesWithFilters(n))
				{
					searchResults.Add(n);
				}
			}

			Nodes.ItemsSource = searchResults;
		}

		private void ResetFiltersButton_Click(object sender, RoutedEventArgs e)
		{
			Nodetype_Combobox.Text = "Any";
			ID_Textbox.Text =
				Prompt_Textbox.Text =
				BoundToActor_Textbox.Text =
				DialogueText_Textbox.Text =
				ActorName_Textbox.Text =
				EventName_Textbox.Text =
				ItemName_Textbox.Text = string.Empty;
			LoadNodeListWithFilters();
		}

		private void ApplyFiltersButton_Click(object sender, RoutedEventArgs e)
		{
			LoadNodeListWithFilters();
		}
	}
}
