using DialogueEditor.Files;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
		const double zoomSpeed = .05;
		bool selectionInProgress = false;
		Point selectionStartPoint;

        public MainWindow()
        {
			instance = this;
            InitializeComponent();
			DialogueDataLine line = new DialogueDataLine("rowName", "dialogueText", "", "", "rowName");

			//Node node1 = new Node(line), node2 = new Node(line);
			//drawArea.Children.Add(node1);
			//drawArea.Children.Add(node2);

			//node1.SetPosition(100, 100);
			//node2.SetPosition(300, 300);
			//Connection u1 = new Connection(node1, node2);
			//drawArea.Children.Add(u1);

			////https://forum.unity3d.com/threads/simple-node-editor.189230/

		}

		
		private void OpenFile(string filePath)
		{
			DeleteAllNodes();

			List<DialogueDataLine> lines = CSVParser.ReadCSV(filePath);
			foreach (var line in lines)
			{
				AddNode(line);
			}
			RefreshNodeConnections();
		}

		private void DeleteAllNodes()
		{
			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				nodes[i].Delete();
			}
			nodes.Clear();
			nodeMap.Clear();
		}

		private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			if(e.Delta>0)
			{
				canvasZoom.ScaleX += zoomSpeed;
				canvasZoom.ScaleY += zoomSpeed;
			}
			else
			{
				canvasZoom.ScaleX -= zoomSpeed;
				canvasZoom.ScaleY -= zoomSpeed;
			}
		}

		private void AddNode(DialogueDataLine data)
		{
			Console.WriteLine("Creating node: " + data.rowName);

			Node n = new Node(data);
			nodeMap.Add(n.nodeNameField.Text, n);
			nodes.Add(n);
			drawArea.Children.Add(n);
		}

		public void DeleteNode(Node node)
		{
			node.DeleteAllOutputConnections();
			drawArea.Children.Remove(node);
			nodeMap.Remove(node.nodeNameField.Text);
			nodes.Remove(node);
			RefreshNodeConnections();
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

		#region Buttons

		private void ButtonNew_Click(object sender, RoutedEventArgs e)
		{
			string filePath = CSVParser.GetFileSaveLocation();
			if (null == filePath) 
			{
				return;
			}

			CSVParser.SaveFile(filePath, new List<Node>());
			OpenFile(filePath);
		}

		private void ButtonOpen_Click(object sender, RoutedEventArgs e)
		{
			string location = CSVParser.GetFileOpenLocation();
			if (location == null)
			{
				return;
			}
			OpenFile(location);
		}

		private void ButtonReload_Click(object sender, RoutedEventArgs e)
		{
			if (CSVParser.filePath == null) 
			{
				MessageBox.Show("First open a file. Only THEN can you reload it.");
				return;
			}
			OpenFile(CSVParser.filePath);
		}

		private void ButtonSave_Click(object sender, RoutedEventArgs e)
		{
			if (CSVParser.filePath == null) 
			{
				ButtonSaveAs_Click(sender, e);
				return;
			}
			CSVParser.SaveFile(CSVParser.filePath, nodes);
		}

		private void ButtonSaveAs_Click(object sender, RoutedEventArgs e)
		{
			string filepath = CSVParser.GetFileSaveLocation();

			if (filepath == null) 
			{
				return;
			}

			ButtonSave_Click(sender, e);
		}

		private void ButtonExport_Click(object sender, RoutedEventArgs e)
		{
			string filepath = CSVParser.GetFileSaveLocation();

			if (filepath == null)
			{
				return;
			}

			CSVParser.ExportFile(filepath, nodes);
		}

		#endregion

		#region Rubberband selection

		private void drawArea_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (Mouse.DirectlyOver != drawArea) 
			{
				return;
			}

			selectionInProgress = true;
			selectionStartPoint = e.GetPosition(drawArea);
			drawArea.CaptureMouse();

			Canvas.SetLeft(selectionBox, selectionStartPoint.X);
			Canvas.SetTop(selectionBox, selectionStartPoint.Y);
			selectionBox.Width = 0;
			selectionBox.Height = 0;

			selectionBox.Visibility = Visibility.Visible;
		}

		private void drawArea_MouseUp(object sender, MouseButtonEventArgs e)
		{
			selectionInProgress = false;
			drawArea.ReleaseMouseCapture();
			selectionBox.Visibility = Visibility.Collapsed;

			Point mouseUpPos = e.GetPosition(drawArea);

			//Check here for nodes intersecting with rect and select them
		}

		private void drawArea_MouseMove(object sender, MouseEventArgs e)
		{
			if(!selectionInProgress)
			{
				return;
			}

			Point mousePos = e.GetPosition(drawArea);

			if(selectionStartPoint.X<mousePos.X)
			{
				Canvas.SetLeft(selectionBox, selectionStartPoint.X);
				selectionBox.Width = mousePos.X - selectionStartPoint.X;
			}
			else
			{
				Canvas.SetLeft(selectionBox, mousePos.X);
				selectionBox.Width = selectionStartPoint.X - mousePos.X;
			}


			if (selectionStartPoint.Y < mousePos.Y)
			{
				Canvas.SetTop(selectionBox, selectionStartPoint.Y);
				selectionBox.Height = mousePos.Y - selectionStartPoint.Y;
			}
			else
			{
				Canvas.SetTop(selectionBox, mousePos.Y);
				selectionBox.Height = selectionStartPoint.Y - mousePos.Y;
			}

		}

		#endregion

	}
}
