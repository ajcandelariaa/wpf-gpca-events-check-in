using Newtonsoft.Json;

namespace GPCAEventsCheckIn.Model
{
    public class OnsiteValidateRequest
    {
        public string PassType { get; set; }
        public string CompanyName { get; set; }
        public string CompanySector { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyCountry { get; set; }
        public string? PromoCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string JobTitle { get; set; }
        public string EmailAddress { get; set; }
        public string ContactNumber { get; set; }
        public string Nationality { get; set; }
    }

    public class OnsiteDraftDelegate
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("job_title")]
        public string JobTitle { get; set; }

        [JsonProperty("company_name")]
        public string CompanyName { get; set; }

        [JsonProperty("badge_type")]
        public string BadgeType { get; set; }
    }

    public class OnsiteDraftPricing
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("unit_price")]
        public decimal UnitPrice { get; set; }

        [JsonProperty("discount_price")]
        public decimal DiscountPrice { get; set; }

        [JsonProperty("net_amount")]
        public decimal NetAmount { get; set; }

        [JsonProperty("total_before_vat")]
        public decimal TotalBeforeVat { get; set; }

        [JsonProperty("vat_price")]
        public decimal VatPrice { get; set; }

        [JsonProperty("total_amount")]
        public decimal TotalAmount { get; set; }
    }

    public class OnsiteValidateData
    {
        [JsonProperty("delegate")]
        public OnsiteDraftDelegate Delegate { get; set; }

        [JsonProperty("pricing")]
        public OnsiteDraftPricing Pricing { get; set; }
    }

    public class OnsiteValidateApiResponse
    {
        [JsonProperty("status")]
        public object Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public OnsiteValidateData Data { get; set; }
    }
}
