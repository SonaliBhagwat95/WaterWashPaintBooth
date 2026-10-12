using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PaintBooth.Models.ERP
{
    public class FrappeOpportunityListResponse
    {
        public List<FrappeOpportunityData> data { get; set; } = new();
    }

    public class FrappeOpportunityData
    {
        public string name { get; set; }              // Opportunity ID, e.g. "2425-ENQ-005"
        public string party_name { get; set; }         // Customer ID
        public string customer_name { get; set; }      // Customer display name
        public string opportunity_from { get; set; }   // "Customer" / "Lead"
        public string status { get; set; }
        [JsonConverter(typeof(FrappeDateTimeConverter))]
        public DateTime? transaction_date { get; set; }

        [JsonConverter(typeof(FrappeDateTimeConverter))]
        public DateTime? creation { get; set; }

        [JsonConverter(typeof(FrappeDateTimeConverter))]
        public DateTime? modified { get; set; }
        public decimal opportunity_amount { get; set; }


        // ── NEW: needed for Create form autofill ──
        public string contact_display { get; set; }
        public string contact_email { get; set; }
        public string contact_mobile { get; set; }
        public string contact_person { get; set; }
        public string customer_address { get; set; }
        public string address_display { get; set; }
        public string customer_location { get; set; }


        public DateTime? SubmittedEnquiryDate { get; set; }
    }
}
