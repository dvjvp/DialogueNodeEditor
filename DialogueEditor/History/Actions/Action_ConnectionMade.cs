using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Pin = System.Windows.FrameworkElement;

namespace DialogueEditor.History.Actions
{
	class Action_ConnectionMade : Action
	{
		Node parentFrom, parentTo;
		Pin objFrom;
		Connection connection;

		public Action_ConnectionMade(Node parentFrom, Pin objFrom, Node parentTo)
		{
			this.parentFrom = parentFrom;
			this.parentTo = parentTo;
			this.objFrom = objFrom;
		}

		public override void Do()
		{
			connection = parentFrom.MakeConnection(parentTo, objFrom);
		}

		public override void Undo()
		{
			parentFrom.outputConnections.Remove(connection);
			parentTo.inputConnections.Remove(connection);
			(connection.Parent as Canvas)?.Children.Remove(connection);
			parentTo.RecalculatePromptAreaVisibility();
			parentFrom.RecalculatePromptAreaVisibility();
		}
	}
}
