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
		public string prompt;
		public string command;
		public string commandArguments;
		public string nextRowName;
		public string boundToActor;

		public double nodePositionX;
		public double nodePositionY;

		public DialogueDataLine()
		{
			rowName = Guid.NewGuid().ToString();
			prompt = "Sample answer";
			command = "dialogue";
			commandArguments = "Sample text";
			nextRowName = "None";
			boundToActor = "None";
		}

		public DialogueDataLine(string rowName, string prompt, string command, string commandArguments, string nextRowName, string boundToActor)
		{
			this.rowName = rowName;
			this.prompt = prompt;
			this.command = command;
			this.commandArguments = commandArguments;
			this.nextRowName = nextRowName;
			this.boundToActor = boundToActor;
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
			string tempPrompt = prompt.Replace("\"", "\"\"");
			string tempArgs = commandArguments.Replace("\"", "\"\"");
			return rowName + ",\"" + tempPrompt + "\",\"" + boundToActor + "\",\"" 
				+ command + "\",\"" + tempArgs + "\",\"" + nextRowName + "\""
				+ "," + (int)nodePositionX + "," + (int)nodePositionY;
		}

		public string ToUE4exportCSVrow()
		{
			string tempPrompt = prompt.Replace("\"", "\"\"");
			string tempArgs = commandArguments.Replace("\"", "\"\"");
			return rowName + ",\"" + tempPrompt + "\",\"" + command + "\",\"" + tempArgs + "\",\"" + nextRowName + "\"";
		}

		public override string ToString()
		{
			return "RowName: " + rowName + ", DialogueText: " + prompt + ", Command: " + command + ", CommandArgs:" + commandArguments + ", NextRow: " + nextRowName;
		}

	}
}
