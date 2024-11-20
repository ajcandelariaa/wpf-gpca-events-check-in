using GPCAEventsCheckIn.ViewModel;
using System.Configuration;
using System.Windows;

namespace GPCAEventsCheckIn.View.Window
{
    public partial class EditAttendeeDetailsView
    {
        private MainViewModel _mainViewModel;
        public EditAttendeeDetailsView(MainViewModel mainViewModel)
        {
            InitializeComponent();
            _mainViewModel = mainViewModel;

            List<string> salutationOptions = new List<string> { "", "Mr.", "Mrs.", "Ms.", "Dr.", "Eng.", "Prof." };

            Cb_Salutation.ItemsSource = salutationOptions;
            Cb_Salutation.SelectedItem = _mainViewModel.CurrentAttendee.Salutation;
            Tb_Fname.Text = _mainViewModel.CurrentAttendee.Fname;
            Tb_Mname.Text = _mainViewModel.CurrentAttendee.Mname;
            Tb_Lname.Text = _mainViewModel.CurrentAttendee.Lname;
            Tb_Jobtitle.Text = _mainViewModel.CurrentAttendee.JobTitle;
            Tb_BadgeType.Text = _mainViewModel.CurrentAttendee.BadgeType;
            Tb_SeatNumber.Text = _mainViewModel.CurrentAttendee.SeatNumber;
        }

        private void Btn_Cancel(object sender, RoutedEventArgs e)
        {
            Application.Current.Windows.OfType<EditAttendeeDetailsView>().FirstOrDefault()?.Close();
            _mainViewModel.BackDropStatus = "Collapsed";
        }

        private async void Btn_Submit(object sender, RoutedEventArgs e)
        {
            _mainViewModel.BackDropStatus = "Visible";
            _mainViewModel.LoadingProgressStatus = "Visible";
            _mainViewModel.LoadingProgressMessage = "Updating...";

            string? salutation = Cb_Salutation.SelectedItem != null ? Cb_Salutation.SelectedItem.ToString() : string.Empty;
            string? firstName = !string.IsNullOrWhiteSpace(Tb_Fname.Text) ? Tb_Fname.Text.Trim() : null;
            string? middleName = !string.IsNullOrWhiteSpace(Tb_Mname.Text) ? Tb_Mname.Text.Trim() : null;
            string? lastName = !string.IsNullOrWhiteSpace(Tb_Lname.Text) ? Tb_Lname.Text.Trim() : null;
            string? jobTitle = !string.IsNullOrWhiteSpace(Tb_Jobtitle.Text) ? Tb_Jobtitle.Text.Trim() : null;
            string? badgeType = !string.IsNullOrWhiteSpace(Tb_BadgeType.Text) ? Tb_BadgeType.Text.Trim() : null;
            string? seatNumber = !string.IsNullOrWhiteSpace(Tb_SeatNumber.Text) ? Tb_SeatNumber.Text.Trim() : null;

            await _mainViewModel.AttendeeViewModel.UpdateDetails(
                ConfigurationManager.AppSettings["ApiCode"],
                _mainViewModel.CurrentAttendee.Id,
                _mainViewModel.CurrentAttendee.DelegateType,
                salutation,
                firstName,
                middleName,
                lastName,
                jobTitle,
                badgeType,
                seatNumber
             );
        }
    }
}
