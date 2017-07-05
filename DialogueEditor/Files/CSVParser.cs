using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
				Console.WriteLine("LineContent: " + lines[i]);

				string[] s = lines[i].Split(',');

				DialogueDataLine d = new DialogueDataLine(s[0], s[1], s[2], s[3], s[4]);
				try
				{
					d.SetPosition(double.Parse(s[5]), double.Parse(s[6]));
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

			if(dialog.ShowDialog() == DialogResult.OK)
			{
				filePath = dialog.FileName;
				return dialog.FileName;
			}
			return null;
		}
	}
}
