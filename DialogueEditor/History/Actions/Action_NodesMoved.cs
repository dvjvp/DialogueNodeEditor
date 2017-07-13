using System.Windows;

namespace DialogueEditor.History.Actions
{
	class Action_NodesMoved : Action
	{
		Node[] nodes;
		Point[] previousLocations;
		Point[] updatedLocations;

		public Action_NodesMoved(Node[] nodes, Point[] previousLocations, Point[] updatedLocations)
		{
			this.nodes = nodes;
			this.previousLocations = previousLocations;
			this.updatedLocations = updatedLocations;
		}

		public override void Do()
		{
			for (int i = 0; i < nodes.Length; i++)
			{
				var node = nodes[i];
				var point = updatedLocations[i];
				node.SetPosition(point.X, point.Y);
				node.ForceConnectionUpdate();
			}
		}

		public override void Undo()
		{
			for (int i = 0; i < nodes.Length; i++)
			{
				var node = nodes[i];
				var point = previousLocations[i];
				node.SetPosition(point.X, point.Y);
				node.ForceConnectionUpdate();
			}
		}
	}
}
