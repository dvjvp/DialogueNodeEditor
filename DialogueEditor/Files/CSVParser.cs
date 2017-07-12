using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace DialogueEditor.Files
{
	static class CSVParser
	{
		public static string filePath;
		public static List<DialogueDataLine> ReadCSV(string filepath)
		{
			string[] lines = File.ReadAllLines(filepath);
			List<DialogueDataLine> list = new List<DialogueDataLine>();

			for (int i = 1 /*Ignore first line*/; i < lines.Length; i++)
			{
				list.Add(ReadCSVLine(lines[i]));
			}

			return list;
		}

		public static DialogueDataLine ReadCSVLine(string line)
		{
			/* Line samples:
			 * RowName,"Comma, Comma, Comma","leave","","None"
			 * RowName2,"Quote"" Quote"" Quote","","","None",100,345
			 */

			string[] s = line.Split(',');
			try
			{
				double.Parse(s[s.Length - 1]);
			}
			catch (Exception)
			{
				s = s.Concat(new string[] { "0", "0" }).ToArray();
			}
			/* From:
			 * 
			 * RowName
			 * "Comma
			 *  Comma
			 *  Comma"
			 * "leave"
			 * ""
			 * "None"
			 * 0 (maybe)
			 * 0 (maybe)
			 */

			/* To:
			 * 
			 * RowName
			 * Comma, Comma, Comma
			 * leave
			 * 
			 * None
			 * 0
			 * 0
			 */ 
			string rowName = s[0];
			string dialogueText = s[1].Replace("\"\"", "\"");
			string command = s[s.Length - 5];
			string commandArgs = s[s.Length - 4];
			string next = s[s.Length - 3];
			double x = 0, y = 0;

			try
			{
				x = double.Parse(s[s.Length-2]);
				y = double.Parse(s[s.Length-1]);
			}
			catch (Exception)
			{
			}

			
			if (dialogueText.StartsWith("\""))
			{
				//combine dialogue into single string
				dialogueText = "";
				int index = 1;

				for (; !s[index].EndsWith("\""); index++) 
				{
					dialogueText += s[index];
					dialogueText += ',';
				}
				dialogueText += s[index];
				dialogueText = dialogueText.Replace("\"\"", "\"");
			}

			//remove unnecessary quote symbols
			if(dialogueText.StartsWith("\""))
			{
				dialogueText = dialogueText.Substring(1, dialogueText.Length - 2);
			}
			if (command.StartsWith("\""))
			{
				command = command.Substring(1, command.Length - 2);
			}
			if (commandArgs.StartsWith("\""))
			{
				commandArgs = commandArgs.Substring(1, commandArgs.Length - 2);
			}
			if (next.StartsWith("\""))
			{
				next = next.Substring(1, next.Length - 2);
			}

			
			DialogueDataLine d = new DialogueDataLine(rowName, dialogueText, command, commandArgs, next);
			d.SetPosition(x, y);
			return d;
		}


		public static string GetFileOpenLocation()
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
			dialog.Title = "Select file to open";
			dialog.Multiselect = false;
			dialog.CheckFileExists = true;
			dialog.DefaultExt = ".csv";

			if (dialog.ShowDialog() == DialogResult.OK)
			{
				filePath = dialog.FileName;
				return dialog.FileName;
			}

			return null;
		}

		public static string GetFileSaveLocation()
		{
			SaveFileDialog dialog = new SaveFileDialog();
			dialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
			dialog.Title = "Save dialogue as...";
			dialog.DefaultExt = ".csv";

			if (dialog.ShowDialog() == DialogResult.OK)
			{
				filePath = dialog.FileName;
				return dialog.FileName;
			}
			return null;
		}

		public static void SaveFile(string filepath, List<Node> nodes)
		{
			if (!filePath.EndsWith(".csv") && !filePath.EndsWith(".CSV"))
			{
				filePath += ".csv";
			}

			foreach (var node in nodes)
			{
				node.ApplyChangesToSourceData();
			}
			foreach (var node in nodes) //Yes, they HAVE to be in 2 separate foreach-es or it won't work properly
			{
				node.ApplyConnectionChangesToSourceData();
			}

			using (StreamWriter outputFile = new StreamWriter(filepath))
			{
				outputFile.WriteLine("---,DialogueText,Command,CommandArguments,Next,X,Y");
				foreach (Node node in nodes)
				{
					outputFile.WriteLine(node.sourceData.ToCSVrow());
				}
			}
		}

		public static void ExportFile(string filepath, List<Node> nodes)
		{
			if (!filePath.EndsWith(".csv") && !filePath.EndsWith(".CSV"))
			{
				filePath += ".csv";
			}

			foreach (var node in nodes)
			{
				node.ApplyChangesToSourceData();
			}
			foreach (var node in nodes) //Yes, they HAVE to be in 2 separate foreach-es or it won't work properly
			{
				node.ApplyConnectionChangesToSourceData();
			}

			using (StreamWriter outputFile = new StreamWriter(filepath))
			{
				outputFile.WriteLine("---,DialogueText,Command,CommandArguments,Next");
				foreach (Node node in nodes)
				{
					outputFile.WriteLine(node.sourceData.ToUE4exportCSVrow());
				}
			}
		}

		public static void GenerateMetadata(string filepath, List<Node> nodes)
		{
			foreach (var node in nodes)
			{
				node.ApplyChangesToSourceData();
			}
			foreach (var node in nodes) //Yes, they HAVE to be in 2 separate foreach-es or it won't work properly
			{
				node.ApplyConnectionChangesToSourceData();
			}

			if (!filePath.EndsWith(".csv") && !filePath.EndsWith(".CSV"))
			{
				filePath += ".csv";
			}
			using (StreamWriter outputFile = new StreamWriter(filepath))
			{
				outputFile.WriteLine("---,ParentActor,WidgetOffset,WidgetText,SequenceToPlay");
				foreach (Node node in nodes)
				{
					outputFile.WriteLine(node.sourceData.rowName + ",\"None\",\"(Rotation=(X=0.000000,Y=-0.000000,Z=0.000000,W=1.000000),Translation=(X=0.000000,Y=0.000000,Z=100.000000),Scale3D=(X=1.000000,Y=1.000000,Z=1.000000))\",\"\",\"None\"");
				}
			}
		}
	}
}
