using GPCAEventsCheckIn.ViewModel;
using System.Windows;

namespace GPCAEventsCheckIn.View.Window
{
    public partial class EditAttendeeDetails
    {
        private AttendeeViewModel _attendeeViewModel;
        private MainViewModel _mainViewModel;
        public EditAttendeeDetails(AttendeeViewModel existingAttendeeViewModel, MainViewModel mainViewModel)
        {
            InitializeComponent();
            _attendeeViewModel = existingAttendeeViewModel;
            _mainViewModel = mainViewModel;

            List<string> salutationOptions = new List<string> { "Mr.", "Mrs.", "Ms.", "Dr.", "Eng.", "Prof." };

            Cb_Salutation.ItemsSource = salutationOptions;
            Cb_Salutation.SelectedItem = _mainViewModel.CurrentAttendee.Salutation;
            Tb_Fname.Text = _mainViewModel.CurrentAttendee.Fname;
            Tb_Mname.Text = _mainViewModel.CurrentAttendee.Mname;
            Tb_Lname.Text = _mainViewModel.CurrentAttendee.Lname;
            Tb_Jobtitle.Text = _mainViewModel.CurrentAttendee.JobTitle;
        }

        private void Btn_Cancel(object sender, RoutedEventArgs e)
        {
            Application.Current.Windows.OfType<EditAttendeeDetails>().FirstOrDefault()?.Close();
            _mainViewModel.BackDropStatus = "Collapsed";
        }

        private void Btn_Submit(object sender, RoutedEventArgs e)
        {

        }
    }
}
