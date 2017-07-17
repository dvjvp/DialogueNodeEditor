using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DialogueEditor.Properties
{
	/// <summary>
	/// Interaction logic for SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow : Window
	{
		public SettingsWindow()
		{
			InitializeComponent();
			AutosaveFrequency.Text = Settings.Default["AutosaveFrequencyMins"].ToString();
			AutosaveFileNum.Text = Settings.Default["AutosaveMaxFiles"].ToString();
			UndoNum.Text = Settings.Default["UndoDepth"].ToString();
			RedoNum.Text = Settings.Default["RedoDepth"].ToString();
		}

		private void ApplyButton_Click(object sender, RoutedEventArgs e)
		{

			Settings.Default["AutosaveFrequencyMins"] = Convert.ToInt32(AutosaveFrequency.Text);
			Settings.Default["AutosaveMaxFiles"] = Convert.ToInt32(AutosaveFileNum.Text);

			Settings.Default["UndoDepth"] = Convert.ToInt32(UndoNum.Text);
			Settings.Default["RedoDepth"] = Convert.ToInt32(RedoNum.Text);

			Settings.Default.Save();

			MessageBox.Show("Some changes will apply only after restarting the program.");
			Close();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void AllowOnlyIntegers(object sender, TextCompositionEventArgs e)
		{
			bool isInputAllowed = true;
			Regex regex = new Regex("[^0-9]+");
			isInputAllowed = !regex.IsMatch((sender as TextBox).Text);
			e.Handled = !isInputAllowed;
		}
	}
}
