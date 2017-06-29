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
        bool isDragNDrop = false;
        Rectangle rect;
        Button butt;
        public MainWindow()
        {
            InitializeComponent();


            rect = new Rectangle();
            rect.Width = 100;    rect.Height = 100;
            rect.Fill = new SolidColorBrush(Colors.Blue);
            rect.Stroke = new SolidColorBrush(Colors.Black);
//             rect.MouseDown += Rect_MouseDown;
//             rect.MouseUp += DrawArea_MouseUp;
//             rect.MouseMove += Rect_MouseMove;

            drawArea.Children.Add(rect);
            Canvas.SetLeft(rect, 10);
            Canvas.SetTop(rect, 10);

            butt = new Button();
            butt.Width = 100; butt.Height = 100;
            butt.Background = new SolidColorBrush(Colors.Blue);
            butt.Foreground = new SolidColorBrush(Colors.Black);

            drawArea.Children.Add(butt);
            Canvas.SetLeft(butt, 210);
            Canvas.SetTop(butt, 210);

            //https://forum.unity3d.com/threads/simple-node-editor.189230/

            //             drawArea.MouseUp += DrawArea_MouseUp;
            //             drawArea.MouseMove += Rect_MouseMove;
        }

        //         private void Rect_MouseMove(object sender, MouseEventArgs e)
        //         {
        //             if (!isDragNDrop) return;
        // 
        //             var mousePos = e.GetPosition(drawArea);
        // 
        //             double left = mousePos.X - (rect.ActualWidth / 2), top = mousePos.Y - (rect.ActualHeight / 2);
        //             Canvas.SetLeft(rect, left);
        //             Canvas.SetTop(rect, top);
        //         }
        // 
        //         private void DrawArea_MouseUp(object sender, MouseButtonEventArgs e)
        //         {
        //             Console.WriteLine("World");
        //             isDragNDrop = false;
        //         }
        // 
        //         private void Rect_MouseDown(object sender, MouseButtonEventArgs e)
        //         {
        //             Console.WriteLine("Hello");
        //             isDragNDrop = true;
        //         }
    }
}
