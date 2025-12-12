using GPCAEventsCheckIn.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Windows;

namespace GPCAEventsCheckIn.ViewModel
{
    public class OnsiteRegistrationViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
        private readonly MainViewModel _mainViewModel;

        private List<string> _members = new();
        private List<string> _countries = new();
        private List<string> _companySectors = new();

        private OnsiteValidateData _currentDraft;
        public OnsiteConfirmResult LastConfirmResult { get; set; }

        public List<string> Members
        {
            get => _members;
            set
            {
                _members = value;
                OnPropertyChanged(nameof(Members));
            }
        }

        public List<string> Countries
        {
            get => _countries;
            set
            {
                _countries = value;
                OnPropertyChanged(nameof(Countries));
            }
        }

        public List<string> CompanySectors
        {
            get => _companySectors;
            set
            {
                _companySectors = value;
                OnPropertyChanged(nameof(CompanySectors));
            }
        }

        public OnsiteRegistrationViewModel(MainViewModel mainViewModel)
        {
            _httpClient = new HttpClient();
            _mainViewModel = mainViewModel;
        }

        public async Task LoadMetadataAsync()
        {
            try
            {
                _mainViewModel.BackDropStatus = "Visible";
                _mainViewModel.LoadingProgressStatus = "Visible";
                _mainViewModel.LoadingProgressMessage = "Fetching onsite metadata...";

                string baseEndpoint = ConfigurationManager.AppSettings["ApiUrlOnsiteRegistration"];

                string url = $"{baseEndpoint}/fetch-metadata";

                var response = await _httpClient.GetStringAsync(url);

                var apiResponse = JsonConvert.DeserializeObject<OnsiteMetadataApiResponse>(response);

                if (apiResponse != null && apiResponse.Status && apiResponse.Data != null)
                {
                    Members = apiResponse.Data.Members ?? new List<string>();
                    Countries = apiResponse.Data.Countries ?? new List<string>();
                    CompanySectors = apiResponse.Data.CompanySectors ?? new List<string>();
                }
                else
                {
                    Members = new List<string>();
                    Countries = new List<string>();
                    CompanySectors = new List<string>();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading onsite metadata: {ex.Message}");
            }
            finally
            {
                _mainViewModel.BackDropStatus = "Collapsed";
                _mainViewModel.LoadingProgressStatus = "Collapsed";
                _mainViewModel.LoadingProgressMessage = "Loading...";
            }
        }

        public OnsiteValidateData CurrentDraft
        {
            get => _currentDraft;
            set
            {
                _currentDraft = value;
                OnPropertyChanged(nameof(CurrentDraft));
            }
        }
        public async Task<bool> ValidateOnsiteRegistrationAsync(OnsiteValidateRequest request)
        {
            try
            {
                _mainViewModel.BackDropStatus = "Visible";
                _mainViewModel.LoadingProgressStatus = "Visible";
                _mainViewModel.LoadingProgressMessage = "Validating onsite registration...";

                string baseEndpoint = ConfigurationManager.AppSettings["ApiUrlOnsiteRegistration"];

                string url = $"{baseEndpoint}/validate";

                var payload = new
                {
                    pass_type = request.PassType,
                    company_name = request.CompanyName,
                    company_sector = request.CompanySector,
                    company_address = request.CompanyAddress,
                    company_country = request.CompanyCountry,
                    promo_code = request.PromoCode,
                    first_name = request.FirstName,
                    last_name = request.LastName,
                    job_title = request.JobTitle,
                    email_address = request.EmailAddress,
                    contact_number = request.ContactNumber,
                    nationality = request.Nationality,

                    pc_name = ConfigurationManager.AppSettings["PCName"],
                    pc_number = ConfigurationManager.AppSettings["PCNumber"]
                };

                string jsonData = JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonConvert.DeserializeObject<OnsiteValidateApiResponse>(responseString);

                if (response.IsSuccessStatusCode)
                {
                    if (apiResponse?.Data == null)
                    {
                        MessageBox.Show("Unexpected response from server while validating onsite registration.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }

                    CurrentDraft = apiResponse.Data;
                    return true;
                }
                else
                {
                    string errorMessage = apiResponse?.Message ?? $"Error validating registration. Status code: {response.StatusCode}";
                    MessageBox.Show(errorMessage, "Validation error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error validating onsite registration: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                _mainViewModel.BackDropStatus = "Collapsed";
                _mainViewModel.LoadingProgressStatus = "Collapsed";
                _mainViewModel.LoadingProgressMessage = "Loading...";
            }
        }

        public async Task<AttendeeModel?> ConfirmOnsiteRegistrationAsync(
    bool isPaid,
    bool sendEmail,
    bool printBadge,
    bool addToApp)
        {
            if (CurrentDraft == null)
            {
                MessageBox.Show("No onsite draft found. Please start again.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            try
            {
                var pcName = ConfigurationManager.AppSettings["PCName"];
                var pcNumber = ConfigurationManager.AppSettings["PCNumber"];

                var payload = new
                {
                    delegate_id = CurrentDraft.Delegate.Id,
                    is_paid = isPaid,
                    send_email = sendEmail,
                    print_badge = printBadge,
                    add_to_app = addToApp,
                    pc_name = pcName,
                    pc_number = pcNumber
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                string baseEndpoint = ConfigurationManager.AppSettings["ApiUrlOnsiteRegistration"];
                var response = await _httpClient.PostAsync(baseEndpoint + "/confirm", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    try
                    {
                        var error = JsonConvert.DeserializeObject<SimpleApiResponse<object>>(responseBody);
                        MessageBox.Show(error?.Message ?? "Error confirming onsite registration.",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch
                    {
                        MessageBox.Show("Error confirming onsite registration.",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    return null;
                }

                try
                {
                    var successResponse =
                        JsonConvert.DeserializeObject<SimpleApiResponse<AttendeeModel>>(responseBody);

                    return successResponse?.Data;
                }
                catch
                {
                    MessageBox.Show("Registration was confirmed, but the response could not be read.",
                        "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error confirming onsite registration: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

    }
}
