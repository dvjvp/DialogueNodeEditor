using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace DialogueEditor.Files
{
	static class CSVParser
	{
		public static string AutosaveLocation
		{
			get
			{
				string path = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
					"Woodpecker dialogue editor",
					"AutosaveData");

				if(Directory.Exists(path) == false)
				{
					Directory.CreateDirectory(path);
				}

				return path;
			}
		}
		public static string filePath;
		public static List<DialogueDataLine> ReadCSV(string filepath)
		{
			string[] lines = File.ReadAllLines(filepath);
			List<DialogueDataLine> list = new List<DialogueDataLine>();

			switch(lines[0])
			{
				case "Woodpecker dialogue file":
					//get version:
					switch(lines[1])
					{
						case "v 0.8":
							for (int i = 3 /*Ignore first 3 lines*/; i < lines.Length; i++)
							{
								list.Add(ReadCSVLine_0_8(lines[i]));
							}
							break;
					}
					break;
				case "---,Prompt,Command,CommandArguments,Next":
					//import file from engine: old file parser should do it.
				case "---,Prompt,Command,CommandArguments,Next,X,Y":
					//open old file
					for (int i = 1 /*Ignore first line*/; i < lines.Length; i++)
					{
						list.Add(ReadCSVLine_0_6(lines[i]));
					}
					break;
			}
			

			return list;
		}

		public static DialogueDataLine ReadCSVLine_0_8(string line)
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
			string prompt = s[1].Replace("\"\"", "\"");
			string boundTo = s[s.Length - 6];
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

			
			if (prompt.StartsWith("\""))
			{
				//combine prompt into single string
				prompt = "";
				int index = 1;
				for (; !s[index].EndsWith("\""); index++) 
				{
					prompt += s[index];
					prompt += ',';
				}
				prompt += s[index];
				index++;

				boundTo = s[index];
				index++;

				command = s[index];
				index++;

				commandArgs = "";
				for (; !s[index].EndsWith("\""); index++)
				{
					commandArgs += s[index];
					commandArgs += ',';
				}
				commandArgs += s[index];
				index++;
			}

			//remove unnecessary quote symbols
			if(prompt.StartsWith("\""))
			{
				prompt = prompt.Substring(1, prompt.Length - 2);
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

			if (boundTo.StartsWith("\""))
			{
				try
				{
					boundTo = boundTo.Substring(1, boundTo.Length - 2);
				}
				catch (Exception)
				{
				}
			}


			prompt = prompt.Replace("\"\"", "\"");
			commandArgs = commandArgs.Replace("\"\"", "\"");


			DialogueDataLine d = new DialogueDataLine(rowName, prompt, command, commandArgs, next, boundTo);
			d.SetPosition(x, y);
			return d;
		}
		public static DialogueDataLine ReadCSVLine_0_6(string line)
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
			string prompt = s[1].Replace("\"\"", "\"");
			string command = s[s.Length - 5];
			string commandArgs = s[s.Length - 4];
			string next = s[s.Length - 3];
			double x = 0, y = 0;

			try
			{
				x = double.Parse(s[s.Length - 2]);
				y = double.Parse(s[s.Length - 1]);
			}
			catch (Exception)
			{
			}


			if (prompt.StartsWith("\""))
			{
				//combine prompt into single string
				prompt = "";
				int index = 1;
				for (; !s[index].EndsWith("\""); index++)
				{
					prompt += s[index];
					prompt += ',';
				}
				prompt += s[index];
				index++;

				command = s[index];
				index++;

				commandArgs = "";
				for (; !s[index].EndsWith("\""); index++)
				{
					commandArgs += s[index];
					commandArgs += ',';
				}
				commandArgs += s[index];
				index++;
			}

			//remove unnecessary quote symbols
			if (prompt.StartsWith("\""))
			{
				prompt = prompt.Substring(1, prompt.Length - 2);
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
			

			prompt = prompt.Replace("\"\"", "\"");
			commandArgs = commandArgs.Replace("\"\"", "\"");


			DialogueDataLine d = new DialogueDataLine(rowName, prompt, command, commandArgs, next, "None");
			d.SetPosition(x, y);
			return d;
		}


		public static string GetFileOpenLocation()
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "Dialogue files (*.dlg)|*dlg|CSV files (*.csv)|*.csv|All files (*.*)|*.*";
			dialog.Title = "Select file to open";
			dialog.Multiselect = false;
			dialog.CheckFileExists = true;
			dialog.DefaultExt = ".dlg";

			if (dialog.ShowDialog() == DialogResult.OK)
			{
				filePath = dialog.FileName;
				return dialog.FileName;
			}

			return null;
		}

		public static string GetFileSaveLocation(bool export = false)
		{
			SaveFileDialog dialog = new SaveFileDialog();
			dialog.Title = "Save dialogue as...";
			if(export)
			{
				dialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
				dialog.DefaultExt = ".csv";
			}
			else
			{
				dialog.Filter = "Dialogue files (*.dlg)|*dlg|CSV files (*.csv)|*.csv|All files (*.*)|*.*";
				dialog.DefaultExt = ".dlg";
			}

			if (dialog.ShowDialog() == DialogResult.OK)
			{
				filePath = dialog.FileName;
				return dialog.FileName;
			}
			return null;
		}

		public static void SaveFile(string filepath, List<Node> nodes)
		{
			if (filepath == null) 
			{
				filepath = Path.Combine(AutosaveLocation, "autosave");
			}

			if (!filepath.EndsWith(".dlg"))
			{
				filepath += ".dlg";
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
				outputFile.WriteLine("Woodpecker dialogue file");
				outputFile.WriteLine("v 0.8");
				outputFile.WriteLine("---,Prompt,Command,CommandArguments,Next,X,Y");
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
				outputFile.WriteLine("---,Prompt,Command,CommandArguments,Next");
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
				outputFile.WriteLine("---,ParentActor,WidgetOffset,SequenceToPlay");
				foreach (Node node in nodes)
				{
					outputFile.WriteLine(node.sourceData.rowName + ",\"" + node.PromptActorsCombobox.Text 
						+ "\",\"(Rotation=(X=0.000000,Y=-0.000000,Z=0.000000,W=1.000000),Translation=(X=0.000000,Y=0.000000,Z=100.000000),Scale3D=(X=1.000000,Y=1.000000,Z=1.000000))\",\"None\"");
				}
			}
		}

		public static string GetAutosaveFilepath()
		{
			string newSavePath = Path.Combine(
				AutosaveLocation,
				(filePath == null ? "autosave" : Path.GetFileName(filePath))
				+ "__"
				+ DateTime.Now.ToShortDateString()
				+ "__"
				+ DateTime.Now.ToShortTimeString().Replace(":", "..")
				+".dlg"
				);


			List<string> s = new List<string>(Directory.GetFiles(AutosaveLocation));
			while (s.Count > 0 && s.Count > (int)Properties.Settings.Default["AutosaveMaxFiles"])
			{
				string oldest = s[0];
				DateTime oldestDate = DateTime.MaxValue;
				foreach (string file in s)
				{
					DateTime createdAt = File.GetLastWriteTime(file);
					if (createdAt < oldestDate)
					{
						oldestDate = createdAt;
						oldest = file;
					}
				}

				File.Delete(oldest);
				s.Remove(oldest);
			}
			

			return newSavePath;
		}

	}
}
