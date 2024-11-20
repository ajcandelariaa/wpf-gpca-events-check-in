using GPCAEventsCheckIn.Model;
using GPCAEventsCheckIn.ViewModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace GPCAEventsCheckIn.View.UserControl
{
    public partial class AttendeeListView
    {
        private MainViewModel _mainViewModel;

        public AttendeeListView(MainViewModel mainViewModel)
        {
            InitializeComponent();
            _mainViewModel = mainViewModel;

            if (_mainViewModel.SelectedCompanyName != null)
            {
                _mainViewModel.AttendeeSuggesstionList = _mainViewModel.AttendeeViewModel.ConfirmedAttendees
                    .Where(delegateItem => string.Equals(delegateItem.CompanyName, _mainViewModel.SelectedCompanyName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            } else
            {
                _mainViewModel.AttendeeSuggesstionList = _mainViewModel.AttendeeViewModel.ConfirmedAttendees.ToList();
            }
        }

        private void Btn_Return(object sender, MouseButtonEventArgs e)
        {
            _mainViewModel.ReturnBack();
            _mainViewModel.SelectedCompanyName = null;

            if(_mainViewModel.AttendeeSuggesstionList != null)
            {
                _mainViewModel.AttendeeSuggesstionList.Clear();
            }
        }

        private void Tb_Keystroke(object sender, TextChangedEventArgs e)
        {
            string searchText = textBoxName.Text.ToLower().Trim();

            IEnumerable<AttendeeModel> filteredList;

            if (_mainViewModel.SelectedCompanyName == null)
            {
                if (string.IsNullOrEmpty(searchText))
                {
                    //filteredList = Enumerable.Empty<AttendeeModel>();
                    filteredList = _mainViewModel.AttendeeViewModel.ConfirmedAttendees;
                }
                else
                {
                    filteredList = _mainViewModel.AttendeeViewModel.ConfirmedAttendees
                        .Where(delegateItem => delegateItem.FullName.ToLower().Contains(searchText) ||
                                               delegateItem.BadgeType.ToLower().Contains(searchText));
                }
            }
            else
            {
                if (string.IsNullOrEmpty(searchText))
                {
                    filteredList = _mainViewModel.AttendeeViewModel.ConfirmedAttendees
                        .Where(delegateItem => string.Equals(delegateItem.CompanyName, _mainViewModel.SelectedCompanyName, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    filteredList = _mainViewModel.AttendeeViewModel.ConfirmedAttendees
                        .Where(delegateItem => string.Equals(delegateItem.CompanyName, _mainViewModel.SelectedCompanyName, StringComparison.OrdinalIgnoreCase))
                        .Where(delegateItem => delegateItem.FullName.ToLower().Contains(searchText) ||
                                               delegateItem.BadgeType.ToLower().Contains(searchText));
                }
            }

            _mainViewModel.AttendeeSuggesstionList = filteredList.ToList();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MyDataGrid.SelectedItem != null)
            {
                AttendeeModel selectedDelegate = (AttendeeModel)MyDataGrid.SelectedItem;
                _mainViewModel.CurrentAttendee = selectedDelegate;
                _mainViewModel.NavigateToAttendeeDetailsView("AttendeeListView");
                _mainViewModel.PreviousView = "AttendeeListView";
            }
        }
    }
}
