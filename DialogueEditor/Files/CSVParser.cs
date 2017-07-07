using System;
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

			bool withCoordinates = lines[0].Contains("X,Y");

			for (int i = 1 /*Ignore first line*/; i < lines.Length; i++)
			{
				Console.WriteLine("LineContent: " + lines[i]);

				string[] s = lines[i].Split(',');
				string dialogue = string.Empty;
				int lastIndex = withCoordinates ? s.Length - 5 : s.Length - 3;
				int j = 1;	//counts how many array elements are consumed by dialogueText.
				for (; j < s.Length - 3; j++) 
				{
					dialogue += s[j];
					dialogue += ',';
				}
				if (dialogue.Length > 2) 
				{
					dialogue = dialogue.Replace("\"\"", "\"");
				}
				//string dialogueText = s[1].Substring(1, s[1].Length - 2);   //don't take '"' signs at the beginning and on the end
				DialogueDataLine d = new DialogueDataLine(s[0],
					dialogue.Substring(1, dialogue.Length - 3),
					s[0 + j].Length > 2 ? s[0 + j].Substring(1, s[0 + j].Length - 2) : "",
					s[1 + j].Length > 2 ? s[1 + j].Substring(1, s[1 + j].Length - 2) : "",
					s[2 + j].Length > 2 ? s[2 + j].Substring(1, s[2 + j].Length - 2) : "");
				try
				{
					d.SetPosition(double.Parse(s[3 + j]), double.Parse(s[4 + j]));
				}
				catch (Exception)
				{
					d.SetPosition(0, 0);
				}
				list.Add(d);
			}

			return list;
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
