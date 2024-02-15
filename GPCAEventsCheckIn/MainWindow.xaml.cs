using GPCAEventsCheckIn.ViewModel;
using System.Windows;

namespace GPCAEventsCheckIn
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent(); 
            DataContext = new MainViewModel();
        }
    }
}