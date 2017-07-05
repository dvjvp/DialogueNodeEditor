using DialogueEditor.Files;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DialogueEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		public static MainWindow instance;
		protected List<Node> nodes = new List<Node>();
		public Dictionary<string, Node> nodeMap = new Dictionary<string, Node>();

        public MainWindow()
        {
			instance = this;
            InitializeComponent();
// 			DialogueDataLine line = new DialogueDataLine("rowName", "dialogueText", "", "", "rowName");
// 
// 			Node node1 = new Node(line), node2 = new Node(line);
// 			drawArea.Children.Add(node1);
// 			drawArea.Children.Add(node2);
// 
// 			node1.SetPosition(100, 100);
// 			node2.SetPosition(300, 300);
// 			Connection u1 = new Connection(node1, node2);
// 			drawArea.Children.Add(u1);
// 
//             //https://forum.unity3d.com/threads/simple-node-editor.189230/

        }

		private void ButtonOpen_Click(object sender, RoutedEventArgs e)
		{
			string location = CSVParser.GetFileOpenLocation();
			if (location == null) 
			{
				return;
			}

			List<DialogueDataLine> lines = CSVParser.ReadCSV(location);
			List<Tuple<string, string>> connectionsToAdd = new List<Tuple<string, string>>();
			foreach (var line in lines)
			{
				AddNode(line);
			}
			//RefreshNodeConnections();
		}

		protected void AddNode(DialogueDataLine data)
		{
			Console.WriteLine("Creating node: " + data.rowName);

			Node n = new Node(data);
			nodeMap.Add(n.nodeNameField.Text, n);
			nodes.Add(n);
			drawArea.Children.Add(n);
		}

		public void DeleteNode(Node node)
		{
			//node.DeleteAllOutputConnections();
			drawArea.Children.Remove(node);
			nodeMap.Remove(node.nodeNameField.Text);
			nodes.Remove(node);
			//RefreshNodeConnections();
		}

		protected void RefreshNodeConnections()
		{
			foreach (var n in nodes)
			{
				n.DeleteAllOutputConnections();
			}
			foreach (var n in nodes)
			{
				n.allConnections.Clear();
			}
			foreach (var n in nodes)
			{
				n.LoadOutputConnectionDataFromSource();
			}
		}
	}
}
