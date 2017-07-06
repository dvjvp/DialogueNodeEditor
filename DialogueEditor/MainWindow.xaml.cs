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
		public List<Node> selection = new List<Node>();

		const double zoomSpeed = .05;
		bool selectionInProgress = false;
		Point selectionStartPoint;

        public MainWindow()
        {
			instance = this;
            InitializeComponent();
			//Based loosely on: https://forum.unity3d.com/threads/simple-node-editor.189230/
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

		public void StartDragnDropSelected(Vector mousePos)
		{
			for (int i = 0; i < selection.Count; i++)
			{
				selection[i].dragOffset = (Vector)selection[i].GetPosition() - mousePos;
			}
		}

		public void DragnDropSelectedOnMove(object sender, MouseEventArgs e)
		{
			var mousePos = e.GetPosition(drawArea);

			for (int i = 0; i < selection.Count; i++)
			{
				var updatedPosition = mousePos + selection[i].dragOffset;
				selection[i].SetPosition(updatedPosition.X, updatedPosition.Y);
				selection[i].ForceConnectionUpdate();
			}
		}

		public void ClearSelection()
		{
			foreach (var node in nodes)
			{
				node.SetSelected(false);
			}
			selection.Clear();
		}

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
			ClearSelection();
		}

		private void drawArea_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if(!selectionInProgress)
			{
				return;
			}
			selectionInProgress = false;
			drawArea.ReleaseMouseCapture();

			Point mouseUpPos = e.GetPosition(drawArea);
			selectionBox.Visibility = Visibility.Collapsed;

			//Check here for nodes intersecting with rect and select them

			foreach (var node in nodes)
			{
				if(AreIntersecting(selectionBox,node))
				{
					selection.Add(node);
					node.SetSelected(true);
				}
			}

		}

		private bool AreIntersecting(Rectangle first, FrameworkElement second)
		{
			Rect r1 = new Rect(Canvas.GetLeft(first), Canvas.GetTop(first), first.Width, first.Height);
			Rect r2 = new Rect(Canvas.GetLeft(second), Canvas.GetTop(second), second.ActualWidth, second.ActualHeight);
			return r1.IntersectsWith(r2);
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
