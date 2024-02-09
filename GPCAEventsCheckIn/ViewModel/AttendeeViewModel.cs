using GPCAEventsCheckIn.Helper;
using GPCAEventsCheckIn.Model;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;

namespace GPCAEventsCheckIn.ViewModel
{
    public class AttendeeViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
        private bool _isLoading;
        private String? _loadingMessage;

        public ObservableCollection<AttendeeModel> ConfirmedAttendees { get; set; }

        public String? LoadingMessage
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

        public ICommand RefreshCommand { get; private set; }

        public AttendeeViewModel()
        {
            _httpClient = new HttpClient();
            ConfirmedAttendees = new ObservableCollection<AttendeeModel>();
            LoadData();
            RefreshCommand = new RelayCommand(ExecuteRefreshCommand);
        }

        public async Task InitializeAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                LoadingMessage = "Fetching data...";
                IsLoading = true;

                ConfirmedAttendees.Clear();

                var response = await _httpClient.GetStringAsync(EventModel.API);
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
        private void ExecuteRefreshCommand()
        {
            LoadData();
        }

        public AttendeeModel? AttendeeDetails(string transactionID)
        {

            for (var i = 0; i < ConfirmedAttendees.Count; i++)
            {
                if (transactionID == ConfirmedAttendees[i].TransactionId)
                {
                    return ConfirmedAttendees[i];
                }
            }
            return null;
        }
    }

    public class ApiResponse
    {
        [JsonProperty("confirmedAttendees")]
        public List<AttendeeModel> ConfirmedAttendees { get; set; }
    }
}
