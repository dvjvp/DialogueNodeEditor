using DialogueEditor.Files;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DialogueEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
			DialogueDataLine line = new DialogueDataLine("rowName", "dialogueText", "", "", "rowName");

			Node node1 = new Node(line), node2 = new Node(line);
			drawArea.Children.Add(node1);
			drawArea.Children.Add(node2);

			node1.SetPosition(100, 100);
			node2.SetPosition(300, 300);
			Connection u1 = new Connection(node1, node2);
			drawArea.Children.Add(u1);

            //https://forum.unity3d.com/threads/simple-node-editor.189230/

        }

    }
}
