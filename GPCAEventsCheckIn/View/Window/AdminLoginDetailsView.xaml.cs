using GPCAEventsCheckIn.ViewModel;
using System.Windows;
using System.Configuration;

namespace GPCAEventsCheckIn.View.Window
{
    public partial class AdminLoginDetailsView
    {
        private MainViewModel _mainViewModel;

        public AdminLoginDetailsView(MainViewModel mainViewModel)
        {
            InitializeComponent();
            _mainViewModel = mainViewModel;
        }

        private void Btn_Cancel(object sender, RoutedEventArgs e)
        {
            Application.Current.Windows.OfType<AdminLoginDetailsView>().FirstOrDefault()?.Close();
            _mainViewModel.BackDropStatus = "Collapsed";
        }

        private void Btn_Submit(object sender, RoutedEventArgs e)
        {
            if(Tb_Username.Text == ConfigurationManager.AppSettings["LocalUsername"] &&
                Tb_Password.Text == ConfigurationManager.AppSettings["LocalPassword"])
            {
                Application.Current.Windows.OfType<AdminLoginDetailsView>().FirstOrDefault()?.Close();
                EditAttendeeDetailsView editAttendeeDetailsView = new EditAttendeeDetailsView(_mainViewModel);
                editAttendeeDetailsView.Owner = Application.Current.MainWindow;
                editAttendeeDetailsView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                //editAttendeeDetailsView.Topmost = true;
                editAttendeeDetailsView.ShowDialog();
            } else
            {
                MessageBox.Show("Invalid username and password!");
            }
        }
    }
}
