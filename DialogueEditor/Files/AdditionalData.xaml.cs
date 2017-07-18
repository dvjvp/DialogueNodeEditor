using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System;
using System.Xml.Serialization;

namespace DialogueEditor.Files
{
	/// <summary>
	/// Interaction logic for AdditionalData.xaml
	/// </summary>
	public partial class AdditionalData : Window
	{
		[Serializable]
		public class DialogueActor
		{
			private string _actorName;
			public string actorName
			{
				get
				{
					return _actorName;
				}
				set
				{
					_actorName = value;
				}
			}
			private string _description;
			public string description
			{
				get
				{
					return _description;
				}
				set
				{
					_description = value;
				}
			}

			public DialogueActor(string objectName, string description)
			{
				this.actorName = objectName;
				this.description = description;
			}
			public DialogueActor()
			{
				actorName = "<<Object name>>";
				description = "<<Description>>";
			}
		}
		[Serializable]
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

		[Serializable]
		public class SerializationData
		{
			public ItemName[] itemNames;
			public DialogueActor[] actors;

			public static SerializationData Create()
			{
				var s = new SerializationData();
				s.itemNames = ItemNames.ToArray();
				s.actors = Actors.ToArray();
				return s;
			}

			public void Load()
			{
				AdditionalData.actors = this.actors.ToList();
				AdditionalData.itemNames = this.itemNames.ToList();
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
				if (actors == null) 
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

		private void LoadDataFromFileButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "Dialogue extra data (*.dlgdt)|*.dlgdt|All files (*.*)|*.*";
			dialog.Title = "Select file to open";
			dialog.Multiselect = false;
			dialog.CheckFileExists = true;
			dialog.DefaultExt = ".dlgdt";

			if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
			{
				return;
			}

			using (var stream = File.OpenRead(dialog.FileName))
			{
				var serializer = new XmlSerializer(typeof(SerializationData));
				(serializer.Deserialize(stream) as SerializationData).Load();
			}

			SetupDataGridReferences();
		}

		private void SaveDataToFileButton_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog dialog = new SaveFileDialog();
			dialog.Filter = "Dialogue extra data (*.dlgdt)|*.dlgdt|All files (*.*)|*.*";
			dialog.Title = "Select save location";
			dialog.DefaultExt = ".dlgdt";

			if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
			{
				return;
			}


			using (var stream = File.Create(dialog.FileName))
			{
				var serializer = new XmlSerializer(typeof(SerializationData));
				serializer.Serialize(stream, SerializationData.Create());
			}
		}
	}
}
