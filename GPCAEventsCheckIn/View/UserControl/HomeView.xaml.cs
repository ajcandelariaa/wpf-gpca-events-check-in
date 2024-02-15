using GPCAEventsCheckIn.Model;
using GPCAEventsCheckIn.View.Window;
using GPCAEventsCheckIn.ViewModel;
using System.Windows;

namespace GPCAEventsCheckIn.View.UserControl
{
    public partial class HomeView
    {
        private MainViewModel _mainViewModel;
        private AttendeeViewModel _attendeeViewModel;

        public HomeView(MainViewModel mainViewModel, AttendeeViewModel existingAttendeeViewModel)
        {
            InitializeComponent();
            _mainViewModel = mainViewModel;
            _attendeeViewModel = existingAttendeeViewModel;
        }

        private void QRScannerClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _mainViewModel.BackDropStatus = "Visible";
            QRCodeScannerView qrScannerView = new QRCodeScannerView(_mainViewModel.AttendeeViewModel, _mainViewModel);
            qrScannerView.Owner = Application.Current.MainWindow;
            qrScannerView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            qrScannerView.Topmost = true;
            qrScannerView.ShowDialog();
        }

        private void SearchCategoriesClicked(object sender, RoutedEventArgs e)
        {
            _mainViewModel.NavigateToSearchCategoryView("HomeView");
        }

        private void Btn_Refresh(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _attendeeViewModel.RefreshData();
        }
    }
}
