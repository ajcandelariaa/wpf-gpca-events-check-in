using GPCAEventsCheckIn.ViewModel;
using System.Windows;
using System.Windows.Media.Imaging;

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