
namespace DialogueEditor.History.Actions
{
	class Action_NodesAdded : Action
	{
		Node[] nodes;

		public Action_NodesAdded(Node[] nodes)
		{
			this.nodes = nodes;
		}

		public override void Do()
		{
			foreach (var node in nodes)
			{
				node.ApplyChangesToSourceData();    //it automatically adds to nodeMap as well
				MainWindow.instance.nodes.Add(node);
				MainWindow.instance.drawArea.Children.Add(node);
			}
		}

		public override void Undo()
		{
			foreach (var node in nodes)
			{
				node.ApplyChangesToSourceData();
			}
			foreach (var node in nodes)
			{
				node.ApplyConnectionChangesToSourceData();
				MainWindow.instance.nodeMap.Remove(node.sourceData.rowName);
				MainWindow.instance.nodes.Remove(node);
				MainWindow.instance.drawArea.Children.Remove(node);
			}
		}
	}
}
