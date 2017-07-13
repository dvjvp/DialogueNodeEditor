using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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



		public static void ClearHistory()
		{
			Instance.undoHistory.Clear();
			Instance.redoHistory.Clear();
		}
		public static void Undo()
		{
			if (Instance.undoHistory.Count > 0) 
			{
				Action a = Instance.undoHistory.Pop();
				a.Undo();
				Instance.redoHistory.Push(a);
			}
		}
		public static void Redo()
		{
			if (Instance.redoHistory.Count > 0) 
			{
				Action a = Instance.redoHistory.Pop();
				a.Do();
				Instance.undoHistory.Push(a);
			}
		}
		public static void Do(Action a)
		{
			a.Do();
			Instance.undoHistory.Push(a);
		}

	}
}
