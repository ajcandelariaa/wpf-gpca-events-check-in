using GPCAEventsCheckIn.ViewModel;
using System.Windows;
using System.Windows.Media;

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