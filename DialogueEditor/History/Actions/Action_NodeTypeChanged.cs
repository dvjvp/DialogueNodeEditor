
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
		}

		public override void Undo()
		{
			node.outputType.Text = previousType;
		}
	}
}
