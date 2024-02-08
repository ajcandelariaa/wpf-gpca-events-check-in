using GPCAEventsCheckIn.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GPCAEventsCheckIn.View.Window
{
    /// <summary>
    /// Interaction logic for SelectCompanyNameView.xaml
    /// </summary>
    public partial class SelectCompanyNameView
    {
        private AttendeeViewModel _attendeeViewModel;
        private MainViewModel _mainViewModel;

        public SelectCompanyNameView(AttendeeViewModel existingAttendeeViewModel, MainViewModel mainViewModel)
        {
            InitializeComponent();
            _attendeeViewModel = existingAttendeeViewModel;
            _mainViewModel = mainViewModel;

            List<string> uniqueCompanyNames = _attendeeViewModel.ConfirmedAttendees
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
