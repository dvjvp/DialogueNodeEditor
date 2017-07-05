using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DialogueEditor.Files
{
	public class DialogueDataLine
	{
		public string rowName;
		public string dialogueText;
		public string command;
		public string commandArguments;
		public string nextRowName;

		public double nodePositionX;
		public double nodePositionY;

		public DialogueDataLine()
		{
		}

		public DialogueDataLine(string rowName, string dialogueText, string command, string commandArguments, string nextRowName)
		{
			this.rowName = rowName;
			this.dialogueText = dialogueText;
			this.command = command;
			this.commandArguments = commandArguments;
			this.nextRowName = nextRowName;
			nodePositionX = 0.0;
			nodePositionY = 0.0;
		}

		public void SetPosition(double X, double Y)
		{
			nodePositionX = X;
			nodePositionY = Y;
		}

		public string ToCSVrow()
		{
			return rowName + ',' + "\"\"\"" + dialogueText + "\"\"\"," + command + ',' + commandArguments + ',' + nextRowName + ',' + nodePositionX + ',' + nodePositionY + '\n';
		}

		public string ToUE4exportCSVrow()
		{
			return rowName + ',' + "\"\"\"" + dialogueText + "\"\"\"," + command + ',' + commandArguments + ',' + nextRowName + '\n';
		}

		public override string ToString()
		{
			return "RowName: " + rowName + ", DialogueText: " + dialogueText + ", Command: " + command + ", CommandArgs:" + commandArguments + ", NextRow: " + nextRowName;
		}

	}
}
