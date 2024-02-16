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

            await _mainViewModel.AttendeeViewModel.UpdateDetails(
                ConfigurationManager.AppSettings["ApiCode"],
                _mainViewModel.CurrentAttendee.Id,
                _mainViewModel.CurrentAttendee.DelegateType,
                salutation,
                Tb_Fname.Text,
                Tb_Mname.Text,
                Tb_Lname.Text,
                Tb_Jobtitle.Text
             );
        }
    }
}
