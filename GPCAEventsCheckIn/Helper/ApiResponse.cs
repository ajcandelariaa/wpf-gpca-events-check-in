using GPCAEventsCheckIn.Model;
using Newtonsoft.Json;

namespace GPCAEventsCheckIn.Helper
{
    public class ApiResponse
    {
        [JsonProperty("confirmedAttendees")]
        public List<AttendeeModel> ConfirmedAttendees { get; set; }
    }
}
