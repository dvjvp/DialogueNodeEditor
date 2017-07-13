
namespace DialogueEditor.History.Actions
{
	class Action_ConnectionMadeGoTo : Action
	{
		Node from, to;
		string previousText;

		public Action_ConnectionMadeGoTo(Node from, Node to)
		{
			this.from = from;
			this.to = to;
		}

		public override void Do()
		{
			previousText = from.TargetDialogueID.Text;
			from.TargetDialogueID.Text = to.sourceData.rowName;
		}

		public override void Undo()
		{
			from.TargetDialogueID.Text = previousText;
		}
	}
}
