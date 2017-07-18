using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DialogueEditor.Files
{
	/// <summary>
	/// Interaction logic for AdditionalData.xaml
	/// </summary>
	public partial class AdditionalData : Window
	{
		public class DialogueActor
		{
			public string actorName { get; set; }
			public string description { get; set; }

			public DialogueActor(string objectName, string description)
			{
				this.actorName = objectName;
				this.description = description;
			}
			public DialogueActor()
			{
				actorName = "<<Object name>>";
				description = "<<Object description>>";
			}
		}
		public class ItemName
		{
			public string Name { get; set; }
			public ItemName(string name)
			{
				Name = name;
			}
			public ItemName()
			{

			}
		}


		private static List<DialogueActor> widgetAnchors;
		private static List<DialogueActor> actors;
		private static List<ItemName> itemNames;
		public static List<DialogueActor> WidgetAnchors
		{
			get
			{
				if(widgetAnchors == null)
				{
					widgetAnchors = new List<DialogueActor>();
				}
				return widgetAnchors;
			}
		}
		public static List<DialogueActor> Actors
		{
			get
			{
				if(actors==null)
				{
					actors = new List<DialogueActor>();
				}
				return actors;
			}
		}
		public static List<ItemName> ItemNames
		{
			get
			{
				if (itemNames == null) 
				{
					itemNames = new List<ItemName>();
				}
				return itemNames;
			}
		}
		
		public static void AddItemName(string name)
		{
			ItemNames.Add(new ItemName(name));
		}
		public static string[] GetItemNames()
		{
			return ItemNames.Select(i => i.Name).ToArray();
		}

		public static void AddDialogueActor(string name)
		{
			Actors.Add(new DialogueActor(name, ""));
		}
		public static string[] GetActorNames()
		{
			return Actors.Select(d => d.actorName).ToArray();
		}

		public static void AddWidgetAnchor(string name)
		{
			WidgetAnchors.Add(new DialogueActor(name, ""));
		}
		public static string[] GetWidgetAnchorNames()
		{
			return WidgetAnchors.Select(i => i.actorName).ToArray();
		}


		public AdditionalData()
		{
			InitializeComponent();
			SetupDataGridReferences();
		}

		private void SetupDataGridReferences()
		{
			WidgetAnchorsDataGrid.ItemsSource = WidgetAnchors;
			ActorNames.ItemsSource = Actors;
			InventoryItemNames.ItemsSource = ItemNames;
		}
	}
}
