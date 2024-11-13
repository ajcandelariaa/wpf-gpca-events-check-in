using Newtonsoft.Json;

namespace GPCAEventsCheckIn.Model
{
    public class AttendeeModel
    {
        //[JsonProperty("accessType")]
        //public string AccessType { get; set; }

        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("delegateType")]
        public string DelegateType { get; set; }

        [JsonProperty("fullName")]
        public string FullName { get; set; }

        [JsonProperty("salutation")]
        public string Salutation { get; set; }

        [JsonProperty("fname")]
        public string Fname { get; set; }

        [JsonProperty("mname")]
        public string Mname { get; set; }

        [JsonProperty("lname")]
        public string Lname { get; set; }

        [JsonProperty("jobTitle")]
        public string JobTitle { get; set; }

        [JsonProperty("companyName")]
        public string CompanyName { get; set; }

        [JsonProperty("badgeType")]
        public string BadgeType { get; set; }

        [JsonProperty("frontText")]
        public string FrontText { get; set; }

        [JsonProperty("frontTextColor")]
        public string FrontTextColor { get; set; }

        [JsonProperty("frontTextBGColor")]
        public string FrontTextBGColor { get; set; }

        [JsonProperty("seatNumber")]
        public string SeatNumber { get; set; }

        [JsonProperty("isPrinted")]
        public string IsPrinted { get; set; }

        [JsonProperty("printedCount")]
        public int PrintedCount { get; set; }
    }

}
