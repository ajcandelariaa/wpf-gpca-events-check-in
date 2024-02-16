using GPCAEventsCheckIn.Helper;
using GPCAEventsCheckIn.Model;
using GPCAEventsCheckIn.View.Window;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Windows;

namespace GPCAEventsCheckIn.ViewModel
{
    public class AttendeeViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
        private MainViewModel _mainViewModel;

        public ObservableCollection<AttendeeModel> ConfirmedAttendees { get; set; }

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
                _mainViewModel.BackDropStatus = "Visible";
                _mainViewModel.LoadingProgressStatus = "Visible";
                _mainViewModel.LoadingProgressMessage = "Fetching data...";

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
                _mainViewModel.BackDropStatus = "Collapsed";
                _mainViewModel.LoadingProgressStatus = "Collapsed";
                _mainViewModel.LoadingProgressMessage = "Loading...";
            }
        }

        public async Task UpdateDetails(string code, int delegateId, string delegateType, string? salutation, string firstName, string middleName, string lastName, string jobTitle)
        {
            try
            {
                var existingAttendeeTemp = _mainViewModel.CurrentAttendee;
                var updatedAttendeeTemp = new AttendeeModel
                {
                    Id = delegateId,
                    DelegateType = delegateType,
                    Salutation = salutation,
                    Fname = firstName,
                    Mname = middleName,
                    Lname = lastName,
                    JobTitle = jobTitle,
                };

                if (!HasChanges(existingAttendeeTemp, updatedAttendeeTemp))
                {
                    _mainViewModel.LoadingProgressStatus = "Collapsed";
                    MessageBox.Show("No changes to update.");
                } else
                {
                    var changes = GetChanges(existingAttendeeTemp, updatedAttendeeTemp);
                    string changesText = GetChangesText(changes);

                    var updateData = new Dictionary<string, string>
                {
                    {"code", code},
                    {"delegateId", delegateId.ToString()},
                    {"delegateType", delegateType},
                    {"salutation", salutation},
                    {"firstName", firstName},
                    {"middleName", middleName},
                    {"lastName", lastName},
                    {"jobTitle", jobTitle},
                    {"pcName", ConfigurationManager.AppSettings["PCName"]},
                    {"pcNumber", ConfigurationManager.AppSettings["PCNumber"]},
                    {"description", changesText}
                };

                    var jsonData = JsonConvert.SerializeObject(updateData);
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync(EventModel.APIEndpoint + "/update-details", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var updatedAttendee = ConfirmedAttendees.FirstOrDefault(a => a.Id == delegateId && a.DelegateType == delegateType);
                        if (updatedAttendee != null)
                        {
                            updatedAttendee.Salutation = salutation;
                            updatedAttendee.Fname = firstName;
                            updatedAttendee.Mname = middleName;
                            updatedAttendee.Lname = lastName;
                            updatedAttendee.JobTitle = jobTitle;

                            string tempFullName = "";

                            if (salutation == "Dr." || salutation == "Prof.")
                            {
                                tempFullName = salutation;

                                if (!string.IsNullOrEmpty(firstName))
                                {
                                    tempFullName += " " + firstName;
                                }
                            }
                            else
                            {
                                tempFullName = firstName;
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
                        _mainViewModel.LoadingProgressStatus = "Collapsed";
                        MessageBox.Show("Details updated successfully");
                        _mainViewModel.BackDropStatus = "Collapsed";
                        Application.Current.Windows.OfType<EditAttendeeDetailsView>().FirstOrDefault()?.Close();
                    }
                    else
                    {
                        _mainViewModel.LoadingProgressStatus = "Collapsed";
                        MessageBox.Show($"Error updating details. Status code: {response.StatusCode}");
                    }
                }
                
            }
            catch (Exception ex)
            {
                _mainViewModel.LoadingProgressStatus = "Collapsed";
                MessageBox.Show($"Error updating details: {ex.Message}");
            }
        }


        public async Task PrintBadge(string code, int delegateId, string delegateType)
        {
            try
            {
                var updateData = new Dictionary<string, string>
                {
                    {"code", code},
                    {"delegateId", delegateId.ToString()},
                    {"delegateType", delegateType},
                    {"pcName", ConfigurationManager.AppSettings["PCName"]},
                    {"pcNumber", ConfigurationManager.AppSettings["PCNumber"]},
                };

                var jsonData = JsonConvert.SerializeObject(updateData);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(EventModel.APIEndpoint + "/print-badge", content);

                if (response.IsSuccessStatusCode)
                {
                    _mainViewModel.LoadingProgressStatus = "Collapsed";
                    MessageBox.Show("Badge printed successfully!");
                    _mainViewModel.BackDropStatus = "Collapsed";
                    _mainViewModel.ReturnBack();
                }
                else
                {
                    _mainViewModel.LoadingProgressStatus = "Collapsed";
                    MessageBox.Show($"Error printing badge. Status code: {response.StatusCode}");
                    _mainViewModel.BackDropStatus = "Collapsed";
                }
            }
            catch (Exception ex)
            {
                _mainViewModel.LoadingProgressStatus = "Collapsed";
                MessageBox.Show($"Error printing badge: {ex.Message}");
                _mainViewModel.BackDropStatus = "Collapsed";
            }
        }

        private bool HasChanges(AttendeeModel existingAttendee, AttendeeModel updatedAttendee)
        {
            return existingAttendee.Salutation != updatedAttendee.Salutation ||
                   existingAttendee.Fname != updatedAttendee.Fname ||
                   existingAttendee.Mname != updatedAttendee.Mname ||
                   existingAttendee.Lname != updatedAttendee.Lname ||
                   existingAttendee.JobTitle != updatedAttendee.JobTitle;
        }

        private Dictionary<string, string> GetChanges(AttendeeModel existingAttendee, AttendeeModel updatedAttendee)
        {
            var changes = new Dictionary<string, string>();

            if (existingAttendee.Salutation != updatedAttendee.Salutation)
            {
                changes.Add("Salutation", $"{existingAttendee.Salutation} -> {updatedAttendee.Salutation}");
            }

            if (existingAttendee.Fname != updatedAttendee.Fname)
            {
                changes.Add("Fname", $"{existingAttendee.Fname} -> {updatedAttendee.Fname}");
            }

            if (existingAttendee.Mname != updatedAttendee.Mname)
            {
                changes.Add("Mname", $"{existingAttendee.Mname} -> {updatedAttendee.Mname}");
            }

            if (existingAttendee.Lname != updatedAttendee.Lname)
            {
                changes.Add("Lname", $"{existingAttendee.Lname} -> {updatedAttendee.Lname}");
            }

            if (existingAttendee.JobTitle != updatedAttendee.JobTitle)
            {
                changes.Add("JobTitle", $"{existingAttendee.JobTitle} -> {updatedAttendee.JobTitle}");
            }

            return changes;
        }

        private string GetChangesText(Dictionary<string, string> changes)
        {
            StringBuilder changesText = new StringBuilder();

            foreach (var change in changes)
            {
                changesText.AppendLine($"{change.Key}: {change.Value}");
            }

            return changesText.ToString();
        }

    }
}
