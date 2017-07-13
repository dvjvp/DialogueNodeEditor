using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

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
				(c.Parent as Canvas)?.Children.Remove(c);
				c.parentTo.RecalculatePromptAreaVisibility();
			}
		}

		public override void Undo()
		{
			foreach (var c in connections)
			{
				c.parentTo.inputConnections.Add(c);
				c.parentFrom.outputConnections.Add(c);
				(c.parentFrom.Parent as Canvas)?.Children.Add(c);
				c.parentTo.RecalculatePromptAreaVisibility();
			}
		}
	}
}
