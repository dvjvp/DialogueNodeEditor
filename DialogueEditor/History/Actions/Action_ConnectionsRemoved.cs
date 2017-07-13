using System;

namespace DialogueEditor.History.Actions
{

	class Action_ConnectionsRemoved : Action
	{
		Connection[] connections;

		public Action_ConnectionsRemoved(Connection[] connections)
		{
			this.connections = connections;
		}

		public override void Do()
		{
			foreach (var c in connections)
			{
				c.parentTo.outputConnections.Remove(c);
				c.parentFrom.outputConnections.Remove(c);
				MainWindow.instance.drawArea.Children.Remove(c);
				//(c.Parent as Canvas)?.Children.Remove(c);
				c.parentTo.RecalculatePromptAreaVisibility();
			}
		}

		public override void Undo()
		{
			foreach (var c in connections)
			{
				c.parentTo.inputConnections.Add(c);
				c.parentFrom.outputConnections.Add(c);
				try
				{
					MainWindow.instance.drawArea.Children.Add(c);
				}
				catch (System.Exception)
				{
					Console.WriteLine("Error 0x00001");
				}
				c.parentTo.RecalculatePromptAreaVisibility();
			}
		}
	}
}
