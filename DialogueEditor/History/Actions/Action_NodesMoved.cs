using System.Windows;

namespace DialogueEditor.History.Actions
{
	class Action_NodesMoved : Action
	{
		string[] nodeIDs;
		Point[] previousLocations;
		Point[] updatedLocations;

		public Action_NodesMoved(string[] nodeIDs, Point[] previousLocations, Point[] updatedLocations)
		{
			this.nodeIDs = nodeIDs;
			this.previousLocations = previousLocations;
			this.updatedLocations = updatedLocations;
		}

		public override void Do()
		{
			for (int i = 0; i < nodeIDs.Length; i++)
			{
				var node = MainWindow.instance.nodeMap[nodeIDs[i]];
				var point = updatedLocations[i];
				node.SetPosition(point.X, point.Y);
				node.ForceConnectionUpdate();
			}
		}

		public override void Undo()
		{
			for (int i = 0; i < nodeIDs.Length; i++)
			{
				var node = MainWindow.instance.nodeMap[nodeIDs[i]];
				var point = previousLocations[i];
				node.SetPosition(point.X, point.Y);
				node.ForceConnectionUpdate();
			}
		}
	}
}
