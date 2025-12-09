namespace NorskHelsenettUpload.Models
{
    // Generated 14.11.2024
    // GET https://orgreg.dev.grunndata.nhn.no/Organization/994598759
    public class OrgRegOrganization
    {
        public string resourceType { get; set; }
        public string id { get; set; }
        public bool active { get; set; }
        public string name { get; set; }
        public Contact[] contact { get; set; }

        public class Contact
        {
            public Address address { get; set; }
        }

        public class Address
        {
            public string use { get; set; }
            public string type { get; set; }
            public string[] line { get; set; }
            public string city { get; set; }
            public string district { get; set; }
            public string postalCode { get; set; }
            public string country { get; set; }
        }

    }
}
