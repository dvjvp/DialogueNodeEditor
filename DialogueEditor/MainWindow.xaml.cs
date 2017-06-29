using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
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


            Rectangle rect = new Rectangle();
            rect.Width = 50;    rect.Height = 50;
            rect.Fill = new SolidColorBrush(Colors.Blue);
            rect.Stroke = new SolidColorBrush(Colors.Black);
            rect.MouseDown += Rect_MouseDown;

            drawArea.Children.Add(rect);

            drawArea.MouseUp += DrawArea_MouseUp;
        }

        private void DrawArea_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("World");
        }

        private void Rect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("Hello");
        }
    }
}
