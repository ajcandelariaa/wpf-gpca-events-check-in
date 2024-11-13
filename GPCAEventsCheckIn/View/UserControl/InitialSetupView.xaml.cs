using GPCAEventsCheckIn.ViewModel;
using System.Configuration;
using System.Windows;

namespace GPCAEventsCheckIn.View.UserControl
{
    public partial class InitialSetupView
    {
        private MainViewModel _mainViewModel;

        public InitialSetupView(MainViewModel mainViewModel)
        {
            InitializeComponent();
            _mainViewModel = mainViewModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string pcName = Tb_PcName.Text;
            string pcNumber = Tb_PcNumber.Text;

            if (string.IsNullOrEmpty(pcName))
            {
                MessageBox.Show("PC Name is required!");
                return;
            }

            if (string.IsNullOrEmpty(pcNumber))
            {
                MessageBox.Show("PC Number is required!");
                return;
            }

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            config.AppSettings.Settings.Add("PCName", pcName);
            config.AppSettings.Settings.Add("PCNumber", pcNumber);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            _mainViewModel.NavigateToHomeView();
        }
    }
}
