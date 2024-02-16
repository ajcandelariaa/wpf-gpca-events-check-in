using GPCAEventsCheckIn.Model;
using GPCAEventsCheckIn.View.Window;
using GPCAEventsCheckIn.ViewModel;
using System.Windows;

namespace GPCAEventsCheckIn.View.UserControl
{
    public partial class HomeView
    {
        private MainViewModel _mainViewModel;

        public HomeView(MainViewModel mainViewModel)
        {
            InitializeComponent();
            _mainViewModel = mainViewModel;
        }

        private void QRScannerClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _mainViewModel.BackDropStatus = "Visible";
            QRCodeScannerView qrScannerView = new QRCodeScannerView(_mainViewModel);
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
            _mainViewModel.AttendeeViewModel.RefreshData();
        }
    }
}
