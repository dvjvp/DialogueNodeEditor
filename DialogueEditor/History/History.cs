using System.Collections.Generic;


namespace DialogueEditor.History
{
	class History
	{
		private static History _instance;
		public static History Instance
		{
			get
			{
				if (_instance == null) 
				{
					_instance = new History();
				}
				return _instance;
			}
		}

		public Stack<Action> undoHistory = new Stack<Action>();
		public Stack<Action> redoHistory = new Stack<Action>();

		public static void UpdateHistoryButtonsVisuals()
		{
			MainWindow.instance.UndoButton.IsEnabled = (Instance.undoHistory.Count > 0);
			MainWindow.instance.RedoButton.IsEnabled = (Instance.redoHistory.Count > 0);
		}

		public static void ClearHistory()
		{
			Instance.undoHistory.Clear();
			Instance.redoHistory.Clear();
			UpdateHistoryButtonsVisuals();
		}
		public static void Undo()
		{
			if (Instance.undoHistory.Count > 0) 
			{
				Action a = Instance.undoHistory.Pop();
				a.Undo();
				Instance.redoHistory.Push(a);
			}
			UpdateHistoryButtonsVisuals();
		}
		public static void Redo()
		{
			if (Instance.redoHistory.Count > 0) 
			{
				Action a = Instance.redoHistory.Pop();
				a.Do();
				Instance.undoHistory.Push(a);
			}
			UpdateHistoryButtonsVisuals();
		}
		public static void Do(Action a)
		{
			Instance.redoHistory.Clear();
			a.Do();
			Instance.undoHistory.Push(a);
			UpdateHistoryButtonsVisuals();
		}
		public static void AddToUndoHistory(Action a)
		{
			Instance.redoHistory.Clear();
			Instance.undoHistory.Push(a);
			UpdateHistoryButtonsVisuals();
		}

	}
}
