
namespace DialogueEditor.History.Actions
{
	class Action_NodeAdded : Action
	{
		Node node;

		public Action_NodeAdded(Node node)
		{
			this.node = node;
		}

		public override void Do()
		{
			node.ApplyChangesToSourceData();	//it automatically adds to nodeMap as well
			MainWindow.instance.nodes.Add(node);
			MainWindow.instance.drawArea.Children.Add(node);
		}

		public override void Undo()
		{
			node.ApplyChangesToSourceData();
			MainWindow.instance.nodeMap.Remove(node.sourceData.rowName);
			MainWindow.instance.nodes.Remove(node);
			MainWindow.instance.drawArea.Children.Remove(node);
		}
	}
}
