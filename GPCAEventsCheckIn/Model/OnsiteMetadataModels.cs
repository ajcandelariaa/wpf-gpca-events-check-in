using Newtonsoft.Json;
using System.Collections.Generic;

namespace GPCAEventsCheckIn.Model
{
    public class OnsiteMetadataData
    {
        [JsonProperty("members")]
        public List<string> Members { get; set; }

        [JsonProperty("countries")]
        public List<string> Countries { get; set; }

        [JsonProperty("companySectors")]
        public List<string> CompanySectors { get; set; }
    }

    public class OnsiteMetadataApiResponse
    {
        [JsonProperty("status")]
        public bool Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public OnsiteMetadataData Data { get; set; }
    }

    public class SimpleApiResponse<T>
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public T Data { get; set; }
    }

    public class OnsiteConfirmResult
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("delegateType")]
        public string DelegateType { get; set; }

        [JsonProperty("registrationStatus")]
        public string RegistrationStatus { get; set; }

        [JsonProperty("paymentStatus")]
        public string PaymentStatus { get; set; }

        [JsonProperty("isPrinted")]
        public string IsPrinted { get; set; }

        [JsonProperty("printedCount")]
        public int PrintedCount { get; set; }
    }
}
