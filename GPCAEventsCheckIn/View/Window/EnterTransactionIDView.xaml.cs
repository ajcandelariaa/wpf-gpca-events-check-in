using GPCAEventsCheckIn.ViewModel;
using System.Windows;

namespace GPCAEventsCheckIn.View.Window
{
    public partial class EnterTransactionIDView
    {
        private AttendeeViewModel _attendeeViewModel;
        private MainViewModel _mainViewModel;
        public EnterTransactionIDView(AttendeeViewModel existingAttendeeViewModel, MainViewModel mainViewModel)
        {
            InitializeComponent();
            _attendeeViewModel = existingAttendeeViewModel;
            _mainViewModel = mainViewModel;
        }

        private void Btn_Cancel(object sender, RoutedEventArgs e)
        {
            Application.Current.Windows.OfType<EnterTransactionIDView>().FirstOrDefault()?.Close();
            _mainViewModel.BackDropStatus = "Collapsed";
        }

        private void Btn_Submit(object sender, RoutedEventArgs e)
        {
            int checker = 0;
            for (int i = 0; i < _attendeeViewModel.ConfirmedAttendees.Count; i++)
            {
                if (Tb_transactionID.Text == _attendeeViewModel.ConfirmedAttendees[i].TransactionId)
                {
                    _mainViewModel.CurrentIndex = i;
                    _mainViewModel.CurrentAttendee = _attendeeViewModel.ConfirmedAttendees[i];
                    checker++;
                    break;
                }
            }
            if (checker > 0)
            {
                _mainViewModel.NavigateToAttendeeDetailsView("SearchCategoriesView");
                Application.Current.Windows.OfType<EnterTransactionIDView>().FirstOrDefault()?.Close();
                _mainViewModel.BackDropStatus = "Collapsed";
            }
            else
            {
                MessageBox.Show("Invalid Transaction ID, Please try again!");
            }
        }
    }
}
