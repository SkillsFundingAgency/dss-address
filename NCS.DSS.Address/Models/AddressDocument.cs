namespace NCS.DSS.Address.Models
{
    public class AddressDocument
    {
        private const string AddressRegEx = @"[A-Za-z0-9 ~!@&amp;'\()*+,\-.\/:;]{1,100}";
        private const string PostcodeRegEx = @"([Gg][Ii][Rr] 0[Aa]{2})|((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9]?[A-Za-z]))))\s?[0-9][A-Za-z]{2})";

        public Guid id { get; set; }

        public Guid? CustomerId { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string Address3 { get; set; }

        public string Address4 { get; set; }

        public string Address5 { get; set; }

        public string PostCode { get; set; }

        public string AlternativePostCode { get; set; }

        public decimal? Longitude { get; set; }

        public decimal? Latitude { get; set; }

        public DateTime? EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        public string LastModifiedTouchpointId { get; set; }

        public string SubcontractorId { get; set; }
    }
}
