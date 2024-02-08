using GPCAEventsCheckIn.View.Window;
using GPCAEventsCheckIn.ViewModel;
using System.Windows;

namespace GPCAEventsCheckIn.View.UserControl
{
    public partial class SearchCategoriesView
    {
        private MainViewModel _mainViewModel;

        public SearchCategoriesView(MainViewModel mainViewModel)
        {
            InitializeComponent();
            _mainViewModel = mainViewModel;
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            _mainViewModel.ReturnBack();
        }

        private void Btn_TransactionId(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _mainViewModel.BackDropStatus = "Visible";
            EnterTransactionIDView enterTransactionIdView = new EnterTransactionIDView(_mainViewModel.AttendeeViewModel, _mainViewModel);
            enterTransactionIdView.Owner = Application.Current.MainWindow;
            enterTransactionIdView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            enterTransactionIdView.Topmost = true;
            enterTransactionIdView.ShowDialog();
        }

        private void Btn_NameOfAttendee(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _mainViewModel.NavigateToAttendeeListView("SearchCategoriesView");
        }

        private void Btn_CompanyName(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _mainViewModel.BackDropStatus = "Visible";
            SelectCompanyNameView selectCompanyNameView = new SelectCompanyNameView(_mainViewModel.AttendeeViewModel, _mainViewModel);
            selectCompanyNameView.Owner = Application.Current.MainWindow;
            selectCompanyNameView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            selectCompanyNameView.Topmost = true;
            selectCompanyNameView.ShowDialog();
        }
    }
}
