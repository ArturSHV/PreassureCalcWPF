using PreassureCalc.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace PreassureCalc.View
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isNormal = true;
        public MainWindow()
        {
            InitializeComponent();
            
            DataContext = new MainViewModel();
        }


        private void Label_MouseDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (isNormal == true)
            {
                WindowState = WindowState.Maximized;
                isNormal = false;
            }
            else
            {
                WindowState = WindowState.Normal;
                isNormal = true;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

    }
}
