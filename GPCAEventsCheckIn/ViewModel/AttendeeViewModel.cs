using GPCAEventsCheckIn.Helper;
using GPCAEventsCheckIn.Model;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Windows;

namespace GPCAEventsCheckIn.ViewModel
{
    public class AttendeeViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
        private bool _isLoading;
        private string? _loadingMessage;
        private MainViewModel _mainViewModel;

        public ObservableCollection<AttendeeModel> ConfirmedAttendees { get; set; }

        public string? LoadingMessage
        {
            get { return _loadingMessage; }
            set
            {
                if (_loadingMessage != value)
                {
                    _loadingMessage = value;
                    OnPropertyChanged(nameof(LoadingMessage));
                }
            }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        public AttendeeViewModel(MainViewModel mainViewModel)
        {
            _httpClient = new HttpClient();
            ConfirmedAttendees = new ObservableCollection<AttendeeModel>();
            _mainViewModel = mainViewModel;
            LoadData();
        }

        public void RefreshData()
        {
            LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                LoadingMessage = "Fetching data...";
                IsLoading = true;

                ConfirmedAttendees.Clear();

                var response = await _httpClient.GetStringAsync(EventModel.APIEndpoint);
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(response);

                ConfirmedAttendees.Clear();

                foreach (var item in apiResponse.ConfirmedAttendees)
                {
                    ConfirmedAttendees.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                LoadingMessage = null;
            }
        }

        public async Task UpdateDetails(string code, int delegateId, string delegateType, string? salutation, string firstName, string middleName, string lastName, string jobTitle)
        {
            try
            {
                var updateData = new Dictionary<string, string>
                {
                    {"code", code},
                    {"delegateId", delegateId.ToString()},
                    {"delegateType", delegateType},
                    {"salutation", salutation},
                    {"firstName", firstName},
                    {"middleName", middleName},
                    {"lastName", lastName},
                    {"jobTitle", jobTitle}
                };

                var jsonData = JsonConvert.SerializeObject(updateData);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(EventModel.APIEndpoint + "/update-details", content);

                if (response.IsSuccessStatusCode)
                {
                    var updatedAttendee = ConfirmedAttendees.FirstOrDefault(a => a.Id == delegateId && a.DelegateType == delegateType);
                    if (updatedAttendee != null)
                    {
                        updatedAttendee.Fname = firstName;
                        updatedAttendee.Mname = middleName;
                        updatedAttendee.Lname = lastName;
                        updatedAttendee.JobTitle = jobTitle;

                        string tempFullName = salutation;
                        if (!string.IsNullOrEmpty(firstName))
                        {
                            tempFullName += " " + firstName;
                        }

                        if (!string.IsNullOrEmpty(middleName))
                        {
                            tempFullName += " " + middleName;
                        }

                        if (!string.IsNullOrEmpty(lastName))
                        {
                            tempFullName += " " + lastName;
                        }

                        updatedAttendee.FullName = tempFullName;

                        _mainViewModel.CurrentAttendee = updatedAttendee;
                    }
                    MessageBox.Show("Details updated successfully");
                }
                else
                {
                    MessageBox.Show($"Error updating details. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating details: {ex.Message}");
            }
        }
    }
}
