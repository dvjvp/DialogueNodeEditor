using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DialogueEditor.History.Actions
{
	class Action_NodeTypeChanged : Action
	{
		Node node;
		string previousType;
		string newType;

		public Action_NodeTypeChanged(Node node, string previousType, string newType)
		{
			this.node = node;
			this.previousType = previousType;
			this.newType = newType;
		}


		public override void Do()
		{
			node.outputType.Text = newType;
			//History.Instance.undoHistory.Pop();	//otherwise it's gonna be added twice, yo
		}

		public override void Undo()
		{
			node.outputType.Text = previousType;
		}
	}
}
