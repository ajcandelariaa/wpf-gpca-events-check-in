using GPCAEventsCheckIn.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GPCAEventsCheckIn.Model;
using System.Linq;

namespace GPCAEventsCheckIn.View.UserControl
{
    public partial class AddAttendeeView
    {

        private MainViewModel _mainViewModel;

        public AddAttendeeView(MainViewModel mainViewModel)
        {
            InitializeComponent();
            Loaded += AddAttendeeView_Loaded;
            _mainViewModel = mainViewModel;
        }

        private async void AddAttendeeView_Loaded(object sender, RoutedEventArgs e)
        {
            Cb_PassType.ItemsSource = new List<string>
            {
                "Member",
                "Non-Member"
            };
            Cb_PassType.SelectedIndex = 0;

            if (_mainViewModel.OnsiteRegistrationViewModel != null)
            {
                await _mainViewModel.OnsiteRegistrationViewModel.LoadMetadataAsync();

                var onsiteVm = _mainViewModel.OnsiteRegistrationViewModel;

                if (onsiteVm.Members != null && onsiteVm.Members.Count > 0)
                {
                    Cb_CompanyName.ItemsSource = onsiteVm.Members;
                }

                if (onsiteVm.CompanySectors != null && onsiteVm.CompanySectors.Count > 0)
                {
                    Cb_CompanySector.ItemsSource = onsiteVm.CompanySectors;
                }

                if (onsiteVm.Countries != null && onsiteVm.Countries.Count > 0)
                {
                    Cb_Country.ItemsSource = onsiteVm.Countries;
                    Cb_Nationality.ItemsSource = onsiteVm.Countries;
                }
            }
        }

        private void Cb_PassType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = Cb_PassType.SelectedItem as string;

            if (string.Equals(selected, "Member", StringComparison.OrdinalIgnoreCase))
            {
                Cb_CompanyName.Visibility = Visibility.Visible;
                Tb_CompanyName.Visibility = Visibility.Collapsed;
                Tb_CompanyName.Text = string.Empty;
            }
            else if (string.Equals(selected, "Non-Member", StringComparison.OrdinalIgnoreCase))
            {
                Cb_CompanyName.Visibility = Visibility.Collapsed;
                Tb_CompanyName.Visibility = Visibility.Visible;
                Cb_CompanyName.SelectedIndex = -1;
            }
        }

        private void Btn_Return(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _mainViewModel.ReturnBack();
        }

        private async void Btn_Next(object sender, RoutedEventArgs e)
        {
            var passType = Cb_PassType.SelectedItem as string;

            if (string.IsNullOrWhiteSpace(passType))
            {
                MessageBox.Show("Please select a pass type.");
                return;
            }

            string companyName = string.Empty;

            if (string.Equals(passType, "Member", StringComparison.OrdinalIgnoreCase))
            {
                if (Cb_CompanyName.SelectedItem == null)
                {
                    MessageBox.Show("Please select a company name.");
                    return;
                }
                companyName = Cb_CompanyName.SelectedItem.ToString();
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Tb_CompanyName.Text))
                {
                    MessageBox.Show("Please enter a company name.");
                    return;
                }
                companyName = Tb_CompanyName.Text.Trim();
            }

            if (Cb_CompanySector.SelectedItem == null)
            {
                MessageBox.Show("Please select a company sector.");
                return;
            }

            if (string.IsNullOrWhiteSpace(Tb_Address.Text))
            {
                MessageBox.Show("Please enter company address.");
                return;
            }

            if (Cb_Country.SelectedItem == null)
            {
                MessageBox.Show("Please select a company country.");
                return;
            }

            if (string.IsNullOrWhiteSpace(Tb_FirstName.Text))
            {
                MessageBox.Show("Please enter first name.");
                return;
            }

            if (string.IsNullOrWhiteSpace(Tb_LastName.Text))
            {
                MessageBox.Show("Please enter last name.");
                return;
            }

            if (string.IsNullOrWhiteSpace(Tb_JobTitle.Text))
            {
                MessageBox.Show("Please enter job title.");
                return;
            }

            if (string.IsNullOrWhiteSpace(Tb_Email.Text))
            {
                MessageBox.Show("Please enter email address.");
                return;
            }

            if (string.IsNullOrWhiteSpace(Tb_ContactNumber.Text))
            {
                MessageBox.Show("Please enter contact number.");
                return;
            }

            if (Cb_Nationality.SelectedItem == null)
            {
                MessageBox.Show("Please select nationality.");
                return;
            }

            var request = new OnsiteValidateRequest
            {
                PassType = passType,
                CompanyName = companyName,
                CompanySector = Cb_CompanySector.SelectedItem.ToString(),
                CompanyAddress = Tb_Address.Text.Trim(),
                CompanyCountry = Cb_Country.SelectedItem.ToString(),
                PromoCode = string.IsNullOrWhiteSpace(Tb_PromoCode.Text)
                    ? null
                    : Tb_PromoCode.Text.Trim(),
                FirstName = Tb_FirstName.Text.Trim(),
                LastName = Tb_LastName.Text.Trim(),
                JobTitle = Tb_JobTitle.Text.Trim(),
                EmailAddress = Tb_Email.Text.Trim(),
                ContactNumber = Tb_ContactNumber.Text.Trim(),
                Nationality = Cb_Nationality.SelectedItem.ToString()
            };

            if (_mainViewModel.OnsiteRegistrationViewModel == null)
            {
                MessageBox.Show("Onsite registration is not initialized.", "Error");
                return;
            }

            bool isValid = await _mainViewModel.OnsiteRegistrationViewModel
                .ValidateOnsiteRegistrationAsync(request);

            if (isValid)
            {
                _mainViewModel.NavigateToOnsiteConfirmView("AddAttendeeView");
            }
        }
    }
}
