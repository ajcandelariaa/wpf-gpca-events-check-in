using GPCAEventsCheckIn.ViewModel;
using System.Windows;

namespace GPCAEventsCheckIn.View.Window
{
    public partial class SelectCompanyNameView
    {
        private MainViewModel _mainViewModel;

        public SelectCompanyNameView(MainViewModel mainViewModel)
        {
            InitializeComponent();
            _mainViewModel = mainViewModel;

            List<string> uniqueCompanyNames = _mainViewModel.AttendeeViewModel.ConfirmedAttendees
            .Select(dm => dm.CompanyName)
            .Distinct()
            .OrderBy(companyName => companyName)
            .ToList();

            Cb_CompanyName.ItemsSource = uniqueCompanyNames;
        }

        private void Btn_Cancel(object sender, RoutedEventArgs e)
        {
            _mainViewModel.SelectedCompanyName = null;
            Application.Current.Windows.OfType<SelectCompanyNameView>().FirstOrDefault()?.Close();
            _mainViewModel.BackDropStatus = "Collapsed";
        }
        private void Btn_Submit(object sender, RoutedEventArgs e)
        {
            if (Cb_CompanyName.SelectedItem != null)
            {
                _mainViewModel.SelectedCompanyName = Cb_CompanyName.SelectedItem.ToString();
                Application.Current.Windows.OfType<SelectCompanyNameView>().FirstOrDefault()?.Close();
                _mainViewModel.NavigateToAttendeeListView("SearchCategoriesView");
                _mainViewModel.BackDropStatus = "Collapsed";
            }
        }
    }
}
