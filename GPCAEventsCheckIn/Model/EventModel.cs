using System.Configuration;

namespace GPCAEventsCheckIn.Model
{
    public class EventModel
    {
        public static string EventCategory = ConfigurationManager.AppSettings["EventCategory"];
        public static string EventYear = ConfigurationManager.AppSettings["EventYear"];
        public static string ApiUrl = ConfigurationManager.AppSettings["ApiUrl"];
        public static string APIEndpoint { get; set; }

        static EventModel()
        {
            APIEndpoint = $"{ApiUrl}{EventCategory}/{EventYear}";
        }
    }
}
