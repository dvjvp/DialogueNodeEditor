
namespace DialogueEditor.History.Actions
{
	class Action_NodesDeleted : Action
	{
		Node[] nodes;

		public Action_NodesDeleted(Node[] nodes)
		{
			this.nodes = nodes;
		}

		public override void Do()
		{
			foreach (var n in nodes)
			{
				MainWindow.instance.drawArea.Children.Remove(n);
				MainWindow.instance.nodeMap.Remove(n.sourceData.rowName);
				MainWindow.instance.nodes.Remove(n);
			}

			MainWindow.instance.RefreshNodeConnections();
		}

		public override void Undo()
		{
			foreach (var n in nodes)
			{
				MainWindow.instance.drawArea.Children.Add(n);
				MainWindow.instance.nodeMap.Add(n.sourceData.rowName, n);
				MainWindow.instance.nodes.Add(n);
			}

			MainWindow.instance.RefreshNodeConnections();
		}
	}
}
