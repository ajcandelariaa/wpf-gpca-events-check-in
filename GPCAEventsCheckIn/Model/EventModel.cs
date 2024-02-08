namespace GPCAEventsCheckIn.Model
{
    public class EventModel
    {
        public static string EventCategory { get; set; } = "ANC";
        public static int EventYear { get; set; } = 2023;
        public static string EventBanner { get; set; } = "Assets/Images/Banner/img_2024_GLF.jpg";
        public static string API { get; set; }

        static EventModel()
        {
            API = $"https://gpcaregistration.com/api/fast-track/{EventCategory}/{EventYear}";
        }
    }
}
