﻿using DialogueEditor.Files;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Diagnostics;

namespace DialogueEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		const double BasicallyInfinity = 99999999999;	//I mean, it's infinity, right? Who even has a screen that big?

		//System variables
		public static MainWindow instance;
		DispatcherTimer autoSaveTimer;

		//Node-related variables
		public List<Node> nodes = new List<Node>();
		public Dictionary<string, Node> nodeMap = new Dictionary<string, Node>();
		public List<Node> selection = new List<Node>();
		public List<Graphics.Comment> comments = new List<Graphics.Comment>();
		bool nodeDragnDropInProgress = false;

		//Zoom
		const double ZOOM_SPEED = .05;
		const double ZOOM_MIN = 0.05;
		const double ZOOM_MAX = 3.0;
		//Pan
		bool panInProgress = false;
		Vector panDragOffset;
		Vector panStartCanvasTranslation;
		//Selection
		bool selectionInProgress = false;
		Point selectionStartPoint;
		Vector selectionStartMousePos;
		Rect selectionRect;
		//Connection drawing
		Node connectionDrawSource;
		FrameworkElement connectionDrawingLineStartPin;

        public MainWindow()
        {
			instance = this;
            InitializeComponent();
			KeyDown += MainWindow_KeyDown;

			drawArea.Width = BasicallyInfinity;
			drawArea.Height = BasicallyInfinity;

			int autoSaveFrequency = (int)Properties.Settings.Default["AutosaveFrequencyMins"];
			if (autoSaveFrequency > 0) //if input is invalid, disable autosave
			{
				autoSaveTimer = new DispatcherTimer();
				autoSaveTimer.Tick += AutoSaveTimer_Tick;
				autoSaveTimer.Interval = new TimeSpan(0, autoSaveFrequency, 0); //ask to save every 10 or so minutes
				autoSaveTimer.Start();
			}

			History.History.UpdateHistoryButtonsVisuals();

			AutoSaveTimer_Tick(null, null);

			string[] args = Environment.GetCommandLineArgs();
			if (args.Length > 1)
			{
				OpenFile(args[1]);
			}
		}
		

		private void AutoSaveTimer_Tick(object sender, EventArgs e)
		{
			string initialFile = CSVParser.filePath;
			CSVParser.SaveFile(CSVParser.GetAutosaveFilepath(), nodes, comments);
			CSVParser.filePath = initialFile;
		}

		private void OpenFile(string filePath)
		{
			MessageLabel.Content = "Opening file...";
			DeleteAllNodes();
			DeleteAllComments();

			List<DialogueDataLine> lines = CSVParser.ReadCSV(filePath);
			foreach (var line in lines)
			{
				if (line.command == "Comment")
				{
					comments.Add(Graphics.Comment.FromDialogueDataLine(line));
				}
				else
				{
					AddNode(line);
				}
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
			//Console.WriteLine("Focus: " + Keyboard.FocusedElement);
			//Do NOT run shortcut menu by pressing F10/alt (windows default), freezing whole application
			if (e.SystemKey == Key.F10 || e.SystemKey == Key.LeftAlt) 
			{
				e.Handled = true;
			}
			Console.WriteLine("Focused on: " + Keyboard.FocusedElement);
			if (Keyboard.FocusedElement != drawArea && Keyboard.FocusedElement != this) 
			{
				return;
			}

			switch(e.Key)
			{
				case Key.Back:
				case Key.Delete:
					DeleteSelectedNodes();
					e.Handled = true;
					break;


				case Key.Z:
					if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
					{
						ButtonUndo_Click(null, null);
						e.Handled = true;
					}
					break;
				case Key.Y:
					if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
					{
						ButtonRedo_Click(null, null);
						e.Handled = true;
					}
					break;

				case Key.C:
					if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
					{
						CopySelectedButton_Click(sender, e);
					}
					else
					{
						ButtonAddComment_Click(null, null);
					}
					break;
				case Key.V:
					if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
					{
						var pasted = PasteNodesFromClipboard();
						if(pasted!=null)
						{
							LayoutManager.MoveCenterTo(pasted, Mouse.GetPosition(drawArea));
						}
					}
					break;

				case Key.Up:
					PanCanvas(0, -30);
					e.Handled = true;
					break;
				case Key.W:
					PanCanvas(0, -30);
					break;
				case Key.Down:
					PanCanvas(0, 30);
					e.Handled = true;
					break;
				case Key.S:
					if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
					{
						ButtonSave_Click(null, null);
					}
					else
					{
						PanCanvas(0, 30);
					}
					break;
				case Key.Right:
					PanCanvas(30, 0);
					e.Handled = true;
					break;
				case Key.D:
					PanCanvas(30, 0);
					break;
				case Key.Left:
					PanCanvas(-30, 0);
					e.Handled = true;
					break;
				case Key.A:
					if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
					{
						ButtonSelectAll_Click(null, null);
					}
					else
					{
						PanCanvas(-30, 0);
					}
					break;
				case Key.F:
					if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
					{
						BringToViewportButton_Click(null, null);
					}
					else
					{
						FocusNodesButton_Click(null, null);
					}
					break;
				case Key.O:
					if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
					{
						ButtonOpen_Click(null, null);
					}
					break;

			}
		}

		private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			Point oldCenter = e.Delta > 0 ? e.GetPosition(drawArea) : GetDrawAreaViewCenter();

			if (e.Delta > 0) 
			{
				canvasZoom.ScaleX += ZOOM_SPEED;
				canvasZoom.ScaleY += ZOOM_SPEED;
			}
			else
			{
				canvasZoom.ScaleX -= ZOOM_SPEED;
				canvasZoom.ScaleY -= ZOOM_SPEED;
			}
			canvasZoom.ScaleX = Math.Max(canvasZoom.ScaleX, ZOOM_MIN);
			canvasZoom.ScaleY = Math.Max(canvasZoom.ScaleY, ZOOM_MIN);
			canvasZoom.ScaleX = Math.Min(canvasZoom.ScaleX, ZOOM_MAX);
			canvasZoom.ScaleY = Math.Min(canvasZoom.ScaleY, ZOOM_MAX);

			Vector offset = e.Delta > 0 ? (oldCenter - e.GetPosition(drawArea)) : (oldCenter - GetDrawAreaViewCenter());

			PanCanvas(offset.X, offset.Y);
		}

		public Point GetDrawAreaViewCenter()
		{
			return canvasTotalTransform.Inverse.Transform(new Point(drawAreaBorder.ActualWidth / 2, drawAreaBorder.ActualHeight / 2));
		}

		#endregion

		#region Mouse Interaction

		private void drawArea_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (Mouse.DirectlyOver == drawArea)
			{
				//drawArea.Focus();	//because of this application crashes on alt+tab. Fix it, yo!
				//Keyboard.ClearFocus();	//fixed crash, but disables keyboard shortcuts :(
				//drawArea.Focus();
				Console.WriteLine("Old: " + Keyboard.FocusedElement);
				Keyboard.ClearFocus();	//take focus from textboxes
				Keyboard.Focus(this);
				FocusManager.SetFocusedElement(this, this);	//take focus from buttons
				//this.Focus();
				//Keyboard.Focus(this);
				Console.WriteLine("New: " + Keyboard.FocusedElement);
			}

			if (e.LeftButton == MouseButtonState.Pressed && !Keyboard.IsKeyDown(Key.LeftAlt))
			{
				if (e.ClickCount == 2 && Mouse.DirectlyOver == drawArea) 
				{
					Point mousePos = e.GetPosition(drawArea);
					var node = AddNode(new DialogueDataLine());
					node.SetPosition(mousePos.X, mousePos.Y);
					node.CreateUniqueID();
					History.History.AddToUndoHistory(new History.Actions.Action_NodesAdded(new Node[] { node }));
				}
				else
				{
					if(!panInProgress)
					{
						StartRubberbandSelection(sender, e);
					}
				}
			}
			else if (e.MiddleButton == MouseButtonState.Pressed
				|| (Keyboard.IsKeyDown(Key.LeftAlt) && e.LeftButton == MouseButtonState.Pressed))
			{
				if(!nodeDragnDropInProgress)
				{
					StartPanCanvas(sender, e);
				}
			}
		}

		private void drawArea_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (selectionInProgress)
			{
				EndRubberbandSelection(sender, e);
			}
			if (panInProgress)
			{
				EndPanCanvas(sender, e);
			}

		}


		private void drawArea_MouseMove(object sender, MouseEventArgs e)
		{
			if (selectionInProgress)
			{
				UpdateRubberbandSelection(sender, e);
			}
			if (panInProgress)
			{
				UpdatePanCanvas(sender, e);
			}


		}

		private void drawArea_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			StartPanCanvas(sender, e);
		}


		#endregion

		#region Node adding and deleting

		public void DeleteComment(Graphics.Comment commentToDelete)
		{
			comments.Remove(commentToDelete);
			drawArea.Children.Remove(commentToDelete);
		}
		private void DeleteAllComments()
		{
			for (int i = comments.Count - 1; i >= 0 ; i--)
			{
				DeleteComment(comments[i]);
			}
		}

		private void ButtonAddNode_Click(object sender, RoutedEventArgs e)
		{
			AddNodeImplementation();
		}

		private Node AddNodeImplementation()
		{
			var node = AddNode(new DialogueDataLine());
			node.CreateUniqueID();
			node.SetPosition(GetDrawAreaViewCenter().X, GetDrawAreaViewCenter().Y);
			History.History.AddToUndoHistory(new History.Actions.Action_NodesAdded(new Node[] { node }));
			return node;
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
			selection = nodes;

			DeleteSelectedNodes();
			
			nodes.Clear();
			nodeMap.Clear();
			selection = new List<Node>();
		}

		private void DeleteSelectedNodes()
		{
			foreach (var n in nodes)
			{
				n.ApplyChangesToSourceData();
			}
			foreach (var n in nodes) //Yes, they HAVE to be in 2 separate foreach-es or it won't work properly
			{
				n.ApplyConnectionChangesToSourceData();
			}

			HashSet<Connection> connectionsToDelete = new HashSet<Connection>();

			foreach (var n in selection)
			{
				n.inputConnections.ForEach(c => connectionsToDelete.Add(c));
				n.outputConnections.ForEach(c => connectionsToDelete.Add(c));
			}

			History.History.Do(new History.Actions.Action_ConnectionsRemoved(connectionsToDelete.ToArray()));

			History.History.Do(new History.Actions.Action_NodesDeleted(selection.ToArray()));
			
			selection.Clear();
		}

		public void RefreshNodeConnections()
		{
			//Delete all connections:
			HashSet<Connection> connectionsToDelete = new HashSet<Connection>();
			foreach (var n in nodes)
			{
				n.inputConnections.ForEach(c => connectionsToDelete.Add(c));
				n.outputConnections.ForEach(c => connectionsToDelete.Add(c));
			}
			( new History.Actions.Action_ConnectionsRemoved(connectionsToDelete.ToArray()) ).Do();

			//And create them anew
			foreach (var n in nodes)
			{
				n.LoadOutputConnectionDataFromSource();
			}
		}

		private void ButtonAddEndDialogueNode_Click(object sender, RoutedEventArgs e)
		{
			AddNodeImplementation().outputType.Text = "End dialogue";
		}

		private void ButtonAddCheckForItemNode_Click(object sender, RoutedEventArgs e)
		{
			AddNodeImplementation().outputType.Text = "If player has item";
		}

		private void ButtonAddMultipleChoicesNode_Click(object sender, RoutedEventArgs e)
		{
			AddNodeImplementation().outputType.Text = "Multiple choices";
		}

		private void ButtonAddLevelEventNode_Click(object sender, RoutedEventArgs e)
		{
			AddNodeImplementation().outputType.Text = "Call level event";
		}

		private void ButtonAddActorEventNode_Click(object sender, RoutedEventArgs e)
		{
			AddNodeImplementation().outputType.Text = "Call actor event";
		}

		private void ButtonAddShortcutNode_Click(object sender, RoutedEventArgs e)
		{
			AddNodeImplementation().outputType.Text = "Shortcut";
		}

		private void ButtonAddShortcutTargetNode_Click(object sender, RoutedEventArgs e)
		{
			AddNodeImplementation().outputType.Text = "Shortcut target";
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
			nodeDragnDropInProgress = true;
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

			nodeDragnDropInProgress = false;
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

			bool selectionAdditive;

			if( Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) )
			{
				selectionAdditive = true;
			}
			else if( Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) )
			{
				selectionAdditive = false;
			}
			else
			{
				selectionAdditive = true;
				ClearSelection();
			}


			Point mouseUpPos = e.GetPosition(drawArea);
			selectionBox.Visibility = Visibility.Collapsed;

			//Check here for nodes intersecting with rect and select them

			foreach (var node in nodes)
			{
				if (AreIntersecting(selectionRect, node))
				{
					if(selectionAdditive)
					{
						selection.Add(node);
						node.SetSelected(true);
					}
					else
					{
						selection.Remove(node);
						node.SetSelected(false);
					}
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

		public void PanCanvas(double x, double y)
		{
			canvasTranslation.X = Math.Min(canvasTranslation.X - x, 0);
			canvasTranslation.Y = Math.Min(canvasTranslation.Y - y, 0);
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

			CSVParser.SaveFile(filePath, new List<Node>(), new List<Graphics.Comment>());
			OpenFile(filePath);

			MessageLabel.Content = "Created file.";
		}

		private void ButtonOpen_Click(object sender, RoutedEventArgs e)
		{
			string location = CSVParser.GetFileOpenLocation();
			if (location == null)
			{
				return;
			}
			OpenFile(location);

			MessageLabel.Content = "Opened file.";
		}

		private void ButtonReload_Click(object sender, RoutedEventArgs e)
		{
			if (CSVParser.filePath == null) 
			{
				MessageBox.Show("First open a file. Only THEN can you reload it.");
				return;
			}
			OpenFile(CSVParser.filePath);

			MessageLabel.Content = "Finished reload.";
		}

		private void ButtonSave_Click(object sender, RoutedEventArgs e)
		{
			if (CSVParser.filePath == null) 
			{
				ButtonSaveAs_Click(sender, e);
				return;
			}
			CSVParser.SaveFile(CSVParser.filePath, nodes, comments);
			MessageLabel.Content = "File saved. " + DateTime.Now.ToShortTimeString();
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
			string previousFilepath = CSVParser.filePath;
			string filepath = CSVParser.GetFileSaveLocation(true);

			if (filepath == null)
			{
				return;
			}

			CSVParser.ExportFile(filepath, nodes);
			CSVParser.filePath = previousFilepath;
			MessageLabel.Content = "Exported file.";
		}

		private void ButtonCreateMetadata_Click(object sender, RoutedEventArgs e)
		{
			string filepath = CSVParser.GetFileSaveLocation(true);
			if (filepath == null)
			{
				return;
			}
			CSVParser.GenerateMetadata(filepath, nodes);
			MessageLabel.Content = "Created metadata.";
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
			if (selection.Count > 1) 
			{
				Point[] previousLocations = selection.Select(n => n.GetPosition()).ToArray();
				LayoutManager.LayoutVertical(selection);
				Point[] newLocations = selection.Select(n => n.GetPosition()).ToArray();
				History.History.Do(new History.Actions.Action_NodesMoved(selection.ToArray(), previousLocations, newLocations));
			}
		}

		private void ButtonLayoutHorizontcal_Click(object sender, RoutedEventArgs e)
		{
			if (selection.Count > 1)
			{
				Point[] previousLocations = selection.Select(n => n.GetPosition()).ToArray();
				LayoutManager.LayoutHorizontal(selection);
				Point[] newLocations = selection.Select(n => n.GetPosition()).ToArray();
				History.History.Do(new History.Actions.Action_NodesMoved(selection.ToArray(), previousLocations, newLocations));
			}
		}

		private void ButtonLayoutAuto_Click(object sender, RoutedEventArgs e)
		{
			if (selection.Count > 1)
			{
				Point[] previousLocations = selection.Select(n => n.GetPosition()).ToArray();
				LayoutManager.LayoutAuto(selection);
				Point[] newLocations = selection.Select(n => n.GetPosition()).ToArray();
				History.History.Do(new History.Actions.Action_NodesMoved(selection.ToArray(), previousLocations, newLocations));
			}
		}

		private void SplitIslands_Click(object sender, RoutedEventArgs e)
		{
			if (selection.Count > 1)
			{
				Point[] previousLocations = selection.Select(n => n.GetPosition()).ToArray();
				LayoutManager.SplitIslands(selection);
				Point[] newLocations = selection.Select(n => n.GetPosition()).ToArray();
				History.History.Do(new History.Actions.Action_NodesMoved(selection.ToArray(), previousLocations, newLocations));
			}
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
		

		private void SendEMailButton_Click(object sender, RoutedEventArgs e)
		{
			Process.Start("mailto:daniel.janowski@thefarm51.com");
		}

		private void AboutInfoButton_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("\"Woodpecker\" Dialogue editor for Chernobyl Game by The Farm 51.\n"
				+"Creator: Daniel Janowski\n"
				+"Version: 0.9.1 Beta\n"
				+"Last changes: 21-07-2017"
				,"Application Info"
				);
		}

		private void SendSparkMessage_Click(object sender, RoutedEventArgs e)
		{
			var result = MessageBox.Show("Write on Spark to daniel.janowski@thefarm51.com\nor press OK to open Spark automatically (experimental).", "Spark message", MessageBoxButton.OKCancel);
			if(result == MessageBoxResult.OK)
			{
				Process.Start("xmpp:daniel.janowski@thefarm51.com");
			}
		}
		
		private void SelectConnected_Click(object sender, RoutedEventArgs e)
		{
			var newSelected = LayoutManager.GetConnected(selection);
			selection.ForEach(n => n.SetSelected(false));
			selection.Clear();
			selection.AddRange(newSelected);
			selection.ForEach(n => n.SetSelected(true));

		}


		private void HelpNavigation_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show(
				"You can move around the canvas (gray area) by dragging with middle mouse button\n" +
				"or with arrows/WSAD keys, when there's no textbox selected.\n" +
				"To deselect any textbox, just click anywhere on empty space on canvas.",
				"Moving around");
			MessageBox.Show(
				"You can select a node by clicking on it with left mouse button\n" +
				"or a group of nodes by drawing selection box. To start drawing selection box " +
				"click and hold left mouse button anywhere on the canvas, to stop drawing selection box," +
				" simply release the button.\n\n" +
				"Selecting with Ctrl will add selected nodes to your current selection, while selecting with shift " +
				"will subtract them from current selection.\n" +
				"Additionally \"Node\" tab near the top of the window contains various options regarding selection.",
				"Selection");
			MessageBox.Show(
				"To create a node, press anywhere on the canvas with your right mouse button.\n" +
				"You can also do this by pressing \"Add node\" button in \"Node\" tab.\n\n" +
				"To delete selected node or nodes, press \"Delete\" or \"Backspace\" button on your keyboard " +
				"or use \"Delete\" button in the \"Node\" tab.",
				"Creating and deleting nodes");
		}

		private void HelpNodes_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("To be implemented");
		}

		private void HelpConnections_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show(
				"To create connection, click on pin for one of the nodes you want to connect.\n" +
				"A light-blue line will appear from that node. Now click on a node you want to connect this pin to.\n\n\n" +
				"Currently there's a bug that makes system not see clicks on anything that is otherwise \"Clickable\" when" +
				" drawing connection, so you have to click on an empty space on node.\n" +
				"There's also another bug, that stops drawing connection line somewhere along the way. To solve it, " +
				"click on the pin you're dragging the connection from, then click again to start making a valid connection."
				, "Creating connections");
			MessageBox.Show(
				"Currently there are 3 ways of deleting connections: \n" +
				"* Select node(-s) and press either \"Delete connections\" (which will delete ALL connections coming from or to " +
				" these nodes) or \"Delete outputs\" (which will delete only connections coming from those nodes) button in \"Node\" tab.\n" +
				"* Alt+Click on a pin with left mouse button to delete all connections coming from/to this pin.\n" +
				"* Alt+Click on a connection (when it's highlighted) to delete this one specific connection."
				, "Deleting connections");
		}

		private void HelpExporting_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show(
				"To export data table to Unreal Engine 4, press \"Export\" button in File tab.\n" +
				"Then import created CSV in Unreal, selecting \"FDialogueData\" as Data Table Struct (You'll be prompt for it.)\n" +
				"If you don't see \"FDialogueData\" on list of structs, it's because UE4 didn't load it yet (UE4 sometimes " +
				"doesn't load some assets on start to save on starting time). To force loading it, click on it before importing " +
				"dialogue data.\nIt can be found in \\Content\\djanowski\\Dialogues folder.\n\n\n" +
				"Beware that in exported CSV all data on node positions is lost, so remember to save your progress beforehand in " +
				"a separate file."
				, "Exporting dialogue data table");
			MessageBox.Show(
				"Metadata is an array consisting of actor references (for displaying dialogue options), level sequence references" +
				" (for playing animations during different dialogue phases) etc. A matching metadata file is REQUIRED for dialogue " +
				"to work in Unreal Engine (it can consist of empty references though).\n\n" +
				"To create metadata table from opened dialogue, press \"Create metadata\" button in File tab.\n" +
				"When importing asset in Unreal Engine, select \"FDialogueMetadata\" as Data Table Struct."
				, "Exporting metadata table");
			MessageBox.Show(
				"For info on how to use those tables, consult Dialogue System tutorial in Tutorial section of our project.",
				"In Unreal");
		}


		private void OpenSettings_Button(object sender, RoutedEventArgs e)
		{
			(new Properties.SettingsWindow()).ShowDialog();
		}

		private void OpenAutosaveLocation_Button(object sender, RoutedEventArgs e)
		{
			try
			{
				Process.Start(CSVParser.AutosaveLocation);
			}
			catch (System.IO.FileNotFoundException)
			{
				MessageBox.Show("Error: couldn't find autosave folder. Try running program as administrator.");
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error: Unknown exception while trying to access autosave location.\n" +
					"Please pass this data to your programmer:\n\n" + ex.Message);
			}
		}

		public void FocusNodesButton_Click(object sender, RoutedEventArgs e)
		{
			if (selection.Count >= 1) 
			{
				Vector viewOffset = LayoutManager.GetCenter(selection) - GetDrawAreaViewCenter();
				PanCanvas(viewOffset.X, viewOffset.Y);
			}
		}

		private void BringToViewportButton_Click(object sender, RoutedEventArgs e)
		{
			if (selection.Count >= 1) 
			{
				Point[] previousLocations = selection.Select(n => n.GetPosition()).ToArray();
				LayoutManager.MoveCenterTo(selection, GetDrawAreaViewCenter());
				Point[] newLocations = selection.Select(n => n.GetPosition()).ToArray();
				History.History.Do(new History.Actions.Action_NodesMoved(selection.ToArray(), previousLocations, newLocations));
			}
		}

		private void OpenDialogueData(object sender, RoutedEventArgs e)
		{
			(new AdditionalData()).ShowDialog();
		}


		private void ButtonAddComment_Click(object sender, RoutedEventArgs e)
		{
			if (selection.Count > 0) 
			{
				comments.Add(Graphics.Comment.Create(LayoutManager.GetBounds(selection), true));
			}
			else
			{
				comments.Add(Graphics.Comment.Create());
			}
		}

		private void OpenFilterWindowButton_Click(object sender, RoutedEventArgs e)
		{
			if (NodeBrowser.instance != null) 
			{
				NodeBrowser.instance.Activate();
			}
			else
			{
				(new NodeBrowser()).Show();
			}
		}


		#endregion

		private void Window_MouseLeave(object sender, MouseEventArgs e)
		{
			//Console.WriteLine(FocusManager.GetFocusedElement(this));
			if (panInProgress)
			{
				EndPanCanvas(sender, null);
			}
		}

		private void CopySelectedButton_Click(object sender, RoutedEventArgs e)
		{
			foreach (var item in selection)
			{
				item.ApplyChangesToSourceData();
			}
			foreach (var item in selection)
			{
				item.ApplyConnectionChangesToSourceData();
			}

			System.Text.StringBuilder builder = new System.Text.StringBuilder();
			foreach (var node in selection)
			{
				builder.AppendLine(node.sourceData.ToCSVrow());
			}
			Clipboard.SetText(builder.ToString());
		}

		private List<Node> PasteNodesFromClipboard()
		{
			string[] buffor = Clipboard.GetText().Split('\n');
			List<Node> pastedNodes = new List<Node>();
			List<DialogueDataLine> copiedData = new List<DialogueDataLine>();
			Dictionary<string, string> newKeys = new Dictionary<string, string>();


			if (buffor == null)
			{
				return null;
			}
			foreach (var line in buffor)
			{
				try
				{
					DialogueDataLine d = CSVParser.ReadCSVLine_0_8(line);
					string newKey = Guid.NewGuid().ToString();
					newKeys.Add(d.rowName, newKey);
					d.rowName = newKey;
					copiedData.Add(d);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}
			foreach (var line in copiedData)
			{
				string[] children = line.nextRowName.Split(' ');
				for (int i = 0; i < children.Length; i++)
				{
					if (newKeys.ContainsKey(children[i]))
					{
						children[i] = newKeys[children[i]];
					}
				}
				line.nextRowName = String.Join(" ", children);
				if (line.command == "options")
				{
					children = line.commandArguments.Split(' ');
					if (children.Length > 1)
					{
						if (newKeys.ContainsKey(children[1]))
						{
							children[1] = newKeys[children[1]];
						}
						line.commandArguments = String.Join(" ", children);
					}
				}
				else if (line.command == "has-item")
				{
					children = line.commandArguments.Split(' ');
					for (int i = 0; i < children.Length; i++)
					{
						if (newKeys.ContainsKey(children[i]))
						{
							children[i] = newKeys[children[i]];
						}
					}
					line.commandArguments = String.Join(" ", children);
				}
			}

			foreach (var item in copiedData)
			{
				Node n = AddNode(item);
				pastedNodes.Add(n);
			}

			foreach (var item in pastedNodes)
			{
				item.LoadOutputConnectionDataFromSource();
			}

			if (pastedNodes.Count > 0) 
			{
				History.History.AddToUndoHistory(new History.Actions.Action_NodesAdded(pastedNodes.ToArray()));

				ClearSelection();
				foreach (var n in pastedNodes)
				{
					n.SetSelected(true);
					selection.Add(n);
				}
			}
			

			return pastedNodes;
		}

		private void PasteSelectedButton_Click(object sender, RoutedEventArgs e)
		{
			PasteNodesFromClipboard();
		}
	}
}
