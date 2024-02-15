using GPCAEventsCheckIn.Helper;
using GPCAEventsCheckIn.Model;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows;

namespace GPCAEventsCheckIn.ViewModel
{
    public class AttendeeViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
        private bool _isLoading;
        private string? _loadingMessage;

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

        public AttendeeViewModel()
        {
            _httpClient = new HttpClient();
            ConfirmedAttendees = new ObservableCollection<AttendeeModel>();
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
    }
}
