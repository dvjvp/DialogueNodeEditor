
namespace DialogueEditor.History
{
	abstract class Action
	{
		public abstract void Do();

		public abstract void Undo();
	}
}
