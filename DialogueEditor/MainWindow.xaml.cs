﻿using DialogueEditor.Files;
using System;
using System.Linq;
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
		//Nodes & System variables
		public static MainWindow instance;
		protected List<Node> nodes = new List<Node>();
		public Dictionary<string, Node> nodeMap = new Dictionary<string, Node>();
		public List<Node> selection = new List<Node>();

		//Zoom, pan & selection
		const double zoomSpeed = .025;
		bool selectionInProgress = false;
		bool panInProgress = false;
		Vector panDragOffset;
		Vector panStartCanvasTranslation;
		Point selectionStartPoint;
		Vector selectionStartMousePos;
		Rect selectionRect;
		Node connectionDrawSource;
		FrameworkElement connectionDrawingLineStartPin;

        public MainWindow()
        {
			instance = this;
            InitializeComponent();
			KeyDown += MainWindow_KeyDown;
			//Based loosely on: https://forum.unity3d.com/threads/simple-node-editor.189230/

			drawArea.Width = 99999999999;	//it's basically infinity right? Who even has screen that big?
			drawArea.Height = 99999999999;
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

			History.History.ClearHistory();
		}

		#region Connection creating

		public void StartDrawingConnection(Node sourceNode, FrameworkElement pin)
		{
			connectionDrawingLine.Visibility = Visibility.Visible;
			connectionDrawSource = sourceNode;
			connectionDrawingLineStartPin = pin;
		}

		public void ConnnectionDrawingOnMouseMoved(object sender, MouseEventArgs args)
		{
			var start = connectionDrawSource.GetPosition() + 
				(Vector)connectionDrawingLineStartPin.TransformToAncestor(connectionDrawSource).Transform(new Point(0, 0));
			start += new Vector(connectionDrawingLineStartPin.ActualWidth/2, connectionDrawingLineStartPin.ActualHeight/2);
			var end = args.GetPosition(drawArea);

			start = canvasTotalTransform.Transform(start);
			end = canvasTotalTransform.Transform(end);

			connectionDrawingLine.X1 = start.X;
			connectionDrawingLine.Y1 = start.Y;
			connectionDrawingLine.X2 = end.X;
			connectionDrawingLine.Y2 = end.Y;
		}

		public void EndDrawingConnection()
		{
			connectionDrawingLine.Visibility = Visibility.Collapsed;
			connectionDrawingLine.X1 = connectionDrawingLine.X2 = connectionDrawingLine.Y1 = connectionDrawingLine.Y2 = 0;
			FrameworkElement mouseOverObject = Mouse.DirectlyOver as FrameworkElement;

			
			Node other = null;
			FrameworkElement transform = mouseOverObject;

			foreach (var item in nodes)
			{
				item.ApplyChangesToSourceData();
			}
			foreach (var item in nodes)
			{
				item.ApplyConnectionChangesToSourceData();
			}

			if (mouseOverObject is RadioButton)
			{
				connectionDrawSource.TryConnecting(connectionDrawingLineStartPin, other, mouseOverObject);
			}
			else
			{
				while (transform != drawArea && transform != null)
				{
					if (transform is Node)
					{
						other = (Node)transform;
						break;
					}
					transform = transform.Parent as FrameworkElement;
				}
				if (other == null)
				{
					return;
				}
				connectionDrawSource.TryConnecting(connectionDrawingLineStartPin, other);
			}
			
		}

		#endregion

		#region Interaction with canvas

		private void MainWindow_KeyDown(object sender, KeyEventArgs e)
		{
			switch(e.Key)
			{
				case Key.Back:
				case Key.Delete:
					DeleteSelectedNodes();
					e.Handled = true;
					break;


				case Key.Z:
					if (Mouse.DirectlyOver != drawArea)
					{
						return;
					}
					if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
					{
						ButtonUndo_Click(null, null);
						e.Handled = true;
					}
					break;
				case Key.Y:
					if (Mouse.DirectlyOver != drawArea)
					{
						return;
					}
					if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
					{
						ButtonRedo_Click(null, null);
						e.Handled = true;
					}
					break;


				case Key.Up:
					if (Mouse.DirectlyOver != drawArea)
					{
						return;
					}
					PanCanvas(0, -30);
					e.Handled = true;
					break;
				case Key.W:
					if (Mouse.DirectlyOver != drawArea)
					{
						return;
					}
					PanCanvas(0, -30);
					break;
				case Key.Down:
					if (Mouse.DirectlyOver != drawArea)
					{
						return;
					}
					PanCanvas(0, 30);
					e.Handled = true;
					break;
				case Key.S:
					if (Mouse.DirectlyOver != drawArea)
					{
						return;
					}
					PanCanvas(0, 30);
					break;
				case Key.Right:
					if (Mouse.DirectlyOver != drawArea)
					{
						return;
					}
					PanCanvas(30, 0);
					e.Handled = true;
					break;
				case Key.D:
					if (Mouse.DirectlyOver != drawArea)
					{
						return;
					}
					PanCanvas(30, 0);
					break;
				case Key.Left:
					if (Mouse.DirectlyOver != drawArea)
					{
						return;
					}
					PanCanvas(-30, 0);
					e.Handled = true;
					break;


				case Key.A:
					if (Mouse.DirectlyOver != drawArea)
					{
						return;
					}
					if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
					{
						ButtonSelectAll_Click(null, null);
					}
					else
					{
						PanCanvas(-30, 0);
					}
					break;

			}
		}

		private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (e.Delta > 0) 
			{
				canvasZoom.ScaleX += zoomSpeed;
				canvasZoom.ScaleY += zoomSpeed;
			}
			else
			{
				canvasZoom.ScaleX -= zoomSpeed;
				canvasZoom.ScaleY -= zoomSpeed;
			}
			canvasZoom.ScaleX = Math.Max(canvasZoom.ScaleX, 0.025);
			canvasZoom.ScaleY = Math.Max(canvasZoom.ScaleY, 0.025);
		}

		#endregion

		#region Node adding and deleting

		private void ButtonAddNode_Click(object sender, RoutedEventArgs e)
		{
			AddNode(new DialogueDataLine()).CreateUniqueID();
		}

		private void drawArea_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			Point mousePos = e.GetPosition(drawArea);
			var node = AddNode(new DialogueDataLine());
			node.SetPosition(mousePos.X, mousePos.Y);
			node.CreateUniqueID();
		}

		private Node AddNode(DialogueDataLine data)
		{
			Console.WriteLine("Creating node: " + data.rowName);

			Node n = new Node(data);
			try
			{
				nodeMap.Add(n.nodeNameField.Text.ToString(), n);
			}
			catch (Exception)
			{
// 				n.nodeNameField.Text = Guid.NewGuid().ToString();
// 				nodeMap.Add(n.nodeNameField.Text, n);
			}
			nodes.Add(n);
			drawArea.Children.Add(n);
			return n;
		}

		private void ButtonDeleteNodes_Click(object sender, RoutedEventArgs e)
		{
			DeleteSelectedNodes();
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

		private void DeleteSelectedNodes()
		{
			for (int i = selection.Count - 1; i >= 0; i--)
			{
				selection[i].Delete();
			}
			selection.Clear();
		}

		public void DeleteNode(Node node)
		{
			foreach (var n in nodes)
			{
				n.ApplyChangesToSourceData();
			}
			foreach (var n in nodes) //Yes, they HAVE to be in 2 separate foreach-es or it won't work properly
			{
				n.ApplyConnectionChangesToSourceData();
			}

			node.DeleteAllConnections();
			drawArea.Children.Remove(node);
			nodeMap.Remove(node.nodeNameField.Text.ToString());
			nodes.Remove(node);
			RefreshNodeConnections();
		}

		protected void RefreshNodeConnections()
		{
			foreach (var n in nodes)
			{
				n.DeleteAllConnections();
			}
			foreach (var n in nodes)
			{
				n.LoadOutputConnectionDataFromSource();
			}
		}

		#endregion

		#region Rubberband selection and multi-node drag'n'drop

		/* DRAG'N'DROP NODES */

		public void StartDragnDropSelected(Vector mousePos)
		{
			selectionStartMousePos = mousePos;
			for (int i = 0; i < selection.Count; i++)
			{
				selection[i].dragOffset = (Vector)selection[i].GetPosition() - mousePos;
			}
			drawArea.Cursor = Cursors.SizeAll;
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
		public void EndDragnDropSelected()
		{
			drawArea.Cursor = Cursors.Arrow;
			foreach (var item in selection)
			{
				item.ApplyChangesToSourceData();
			}
			
			Point[] nodeStartingPositions = selection.Select(n =>(Point)(n.dragOffset + selectionStartMousePos)).ToArray();
			Point[] nodeEndPositions = selection.Select(n => n.GetPosition()).ToArray();

			if (selection.Count > 0)
			{
				double deltaMovement = ((Vector)(nodeEndPositions[0] - (Vector)nodeStartingPositions[0])).Length;
				//to fix a bug, where just clicking on a node would add a record in history "moved by 0".
				if(deltaMovement > 0)
				{
					History.Actions.Action_NodesMoved nodesMovedAction = new History.Actions.Action_NodesMoved(selection.ToArray(), nodeStartingPositions, nodeEndPositions);
					History.History.Do(nodesMovedAction);
				}
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

		/* RUBBERBAND SELECTION */

		private void StartRubberbandSelection(object sender, MouseButtonEventArgs e)
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
			selectionRect.X = 0;
			selectionRect.Y = 0;
			selectionRect.Width = 0;
			selectionRect.Height = 0;

			selectionBox.Visibility = Visibility.Visible;
			ClearSelection();
		}
		private void UpdateRubberbandSelection(object sender, MouseEventArgs e)
		{
			Point mousePos = e.GetPosition(drawArea);
			Point tMousePos = canvasTotalTransform.Transform(mousePos);
			Point tSelectionStartPoint = canvasTotalTransform.Transform(this.selectionStartPoint);

			if (selectionStartPoint.X < mousePos.X)
			{
				Canvas.SetLeft(selectionBox, tSelectionStartPoint.X);
				selectionBox.Width = tMousePos.X - tSelectionStartPoint.X;
				selectionRect.X = selectionStartPoint.X;
				selectionRect.Width = mousePos.X - selectionStartPoint.X;
			}
			else
			{
				Canvas.SetLeft(selectionBox, tMousePos.X);
				selectionBox.Width = tSelectionStartPoint.X - tMousePos.X;
				selectionRect.X = mousePos.X;
				selectionRect.Width = selectionStartPoint.X - mousePos.X;
			}


			if (selectionStartPoint.Y < mousePos.Y)
			{
				Canvas.SetTop(selectionBox, tSelectionStartPoint.Y);
				selectionBox.Height = tMousePos.Y - tSelectionStartPoint.Y;
				selectionRect.Y = selectionStartPoint.Y;
				selectionRect.Height = mousePos.Y - selectionStartPoint.Y;
			}
			else
			{
				Canvas.SetTop(selectionBox, tMousePos.Y);
				selectionBox.Height = tSelectionStartPoint.Y - tMousePos.Y;
				selectionRect.Y = mousePos.Y;
				selectionRect.Height = selectionStartPoint.Y - mousePos.Y;
			}
		}
		private void EndRubberbandSelection(object sender, MouseButtonEventArgs e)
		{
			selectionInProgress = false;
			drawArea.ReleaseMouseCapture();

			Point mouseUpPos = e.GetPosition(drawArea);
			selectionBox.Visibility = Visibility.Collapsed;

			//Check here for nodes intersecting with rect and select them

			foreach (var node in nodes)
			{
				if (AreIntersecting(selectionRect, node))
				{
					selection.Add(node);
					node.SetSelected(true);
				}
			}

		}

		private bool AreIntersecting(Rect first, FrameworkElement second)
		{
			//Rect r1 = new Rect(Canvas.GetLeft(first), Canvas.GetTop(first), first.Width, first.Height);
			Rect r2 = new Rect(Canvas.GetLeft(second), Canvas.GetTop(second), second.ActualWidth, second.ActualHeight);
			return first.IntersectsWith(r2);
		}

		#endregion

		#region Pan canvas with middle mouse button or arrow keys

		/* PAN BY DRAGGING W/ MIDDLE MOUSE BUTTON */

		private void StartPanCanvas(object sender, MouseButtonEventArgs e)
		{
			//Console.WriteLine("Started pan");
			panInProgress = true;
			panDragOffset = (Vector)e.GetPosition(this);
			panStartCanvasTranslation = new Vector(canvasTranslation.X, canvasTranslation.Y);
			drawArea.Cursor = Cursors.ScrollAll;
		}
		private void UpdatePanCanvas(object sender, MouseEventArgs e)
		{
			var totalDeltaDrag = e.GetPosition(this) - panDragOffset;
			//Console.WriteLine(totalDeltaDrag);

			canvasTranslation.X = Math.Min(panStartCanvasTranslation.X + totalDeltaDrag.X, 0);
			canvasTranslation.Y = Math.Min(panStartCanvasTranslation.Y + totalDeltaDrag.Y, 0);

		}
		private void EndPanCanvas(object sender, MouseButtonEventArgs e)
		{
			//Console.WriteLine("Stopped pan");
			panInProgress = false;
			drawArea.Cursor = Cursors.Arrow;
		}

		private void PanCanvas(double x, double y)
		{
			canvasTranslation.X = Math.Min(canvasTranslation.X - x, 0);
			canvasTranslation.Y = Math.Min(canvasTranslation.Y - y, 0);
		}

		#endregion

		#region Mouse Interaction


		private void drawArea_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				StartRubberbandSelection(sender, e);
			}
			else if (e.MiddleButton == MouseButtonState.Pressed) 
			{
				StartPanCanvas(sender, e);
			}
		}

		private void drawArea_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (selectionInProgress)
			{
				EndRubberbandSelection(sender, e);
			}
			else if (panInProgress) 
			{
				EndPanCanvas(sender, e);
			}
			
		}


		private void drawArea_MouseMove(object sender, MouseEventArgs e)
		{
			if(selectionInProgress)
			{
				UpdateRubberbandSelection(sender, e);
			}
			if(panInProgress)
			{
				UpdatePanCanvas(sender, e);
			}

			
		}



		#endregion

		#region Buttons

		/* FILE */

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

		private void ButtonCreateMetadata_Click(object sender, RoutedEventArgs e)
		{
			string filepath = CSVParser.GetFileSaveLocation();
			if (filepath == null)
			{
				return;
			}
			CSVParser.GenerateMetadata(filepath, nodes);
		}

		private void ButtonTest_Click(object sender, RoutedEventArgs e)
		{
			if (selection.Count != 1)
			{
				MessageBox.Show("Please, select a single node, from which to start the dialogue.");
				return;
			}

			foreach (var node in nodes)
			{
				node.ApplyChangesToSourceData();
			}
			foreach (var node in nodes)
			{
				node.ApplyConnectionChangesToSourceData();
			}

			var window = new Testing.TestingWindow(nodeMap, selection[0]);
			window.Show();
		}

		/* NODE */

		private void ButtonDeleteConnections_Click(object sender, RoutedEventArgs e)
		{
			foreach (var node in selection)
			{
				node.DeleteAllConnections();
			}
		}

		private void ButtonDeleteOutputs_Click(object sender, RoutedEventArgs e)
		{
			foreach (var node in selection)
			{
				node.DeleteAllOutputConnections();
			}
		}

		private void ButtonSelectAll_Click(object sender, RoutedEventArgs e)
		{
			foreach (var node in nodes)
			{
				selection.Add(node);
				node.SetSelected(true);
			}
		}

		private void ButtonDeselectAll_Click(object sender, RoutedEventArgs e)
		{
			ClearSelection();
		}

		private void ButtonLayoutVertical_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Not implemented yet");
		}

		private void ButtonLayoutHorizontcal_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Not implemented yet");
		}

		private void ButtonLayoutAuto_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Not implemented yet");
		}

		/* HISTORY */

		private void ButtonUndo_Click(object sender, RoutedEventArgs e)
		{
			History.History.Undo();
		}

		private void ButtonRedo_Click(object sender, RoutedEventArgs e)
		{
			History.History.Redo();
		}



		#endregion

	}
}
