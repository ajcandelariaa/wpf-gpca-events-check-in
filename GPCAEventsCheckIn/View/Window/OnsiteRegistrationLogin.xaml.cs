using GPCAEventsCheckIn.ViewModel;
using System.Configuration;
using System.Windows;

namespace GPCAEventsCheckIn.View.Window
{
    public partial class OnsiteRegistrationLogin
    {
        private MainViewModel _mainViewModel;
        public OnsiteRegistrationLogin(MainViewModel mainViewModel)
        {
            InitializeComponent();
            _mainViewModel = mainViewModel;
        }

        private void Btn_Submit(object sender, RoutedEventArgs e)
        {
            string password = Pb_Password.Password;

            if (password == ConfigurationManager.AppSettings["OnsiteRegistrationPassword"])
            {
                _mainViewModel.NavigateToAddAttendeeView("SearchCategoriesView");
                Application.Current.Windows.OfType<OnsiteRegistrationLogin>().FirstOrDefault()?.Close();
                _mainViewModel.BackDropStatus = "Collapsed";
            }
            else
            {
                MessageBox.Show(
                    "Incorrect password. Please try again.",
                    "Access Denied",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                Pb_Password.Clear();
                Pb_Password.Focus();
            }
        }

        private void Btn_Cancel(object sender, RoutedEventArgs e)
        {
            Application.Current.Windows.OfType<OnsiteRegistrationLogin>().FirstOrDefault()?.Close();
            _mainViewModel.BackDropStatus = "Collapsed";
        }
    }
}
