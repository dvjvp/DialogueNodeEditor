using DialogueEditor.Files;

namespace DialogueEditor.History.Actions
{
	class Action_NodeDataChanged : Action
	{
		Node node;
		DialogueDataLine oldData, newData;

		public Action_NodeDataChanged(Node node, DialogueDataLine oldData, DialogueDataLine newData)
		{
			this.node = node;
			this.oldData = oldData;
			this.newData = newData;
		}

		public override void Do()
		{
			node.sourceData = newData;
			node.LoadDataFromSource(true);
		}

		public override void Undo()
		{
			node.sourceData = oldData;
			node.LoadDataFromSource(true);
		}
	}
}
