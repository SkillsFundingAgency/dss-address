using System;
using System.ComponentModel.DataAnnotations;
using DFC.JSON.Standard.Attributes;
using DFC.Swagger.Standard.Annotations;

namespace NCS.DSS.Address.Models
{
    public class Address : IAddress
    {
        private const string AddressRegEx = @"[A-Za-z0-9 ~!@&amp;'\()*+,\-.\/:;]{1,100}";
        private const string PostcodeRegEx = @"([Gg][Ii][Rr] 0[Aa]{2})|((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9]?[A-Za-z]))))\s?[0-9][A-Za-z]{2})";

        [Display(Description = "Unique identifier for an address.")]
        [Example(Description = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85")]
        [Newtonsoft.Json.JsonProperty(PropertyName = "id")]
        public Guid? AddressId { get; set; }

        [Display(Description = "Unique identifier of a customer")]
        [Example(Description = "2730af9c-fc34-4c2b-a905-c4b584b0f379")]
        public Guid? CustomerId { get; set; }

        [Required]
        [RegularExpression(AddressRegEx)]
        [Display(Description = "Customer home address line 1")]
        [Example(Description = "Adddress Line 1")]
        public string Address1 { get; set; }

        [RegularExpression(AddressRegEx)]
        [Display(Description = "Customer home address line 2")]
        [Example(Description = "Adddress Line 2")]
        public string Address2 { get; set; }

        [RegularExpression(AddressRegEx)]
        [Display(Description = "Customer home address line 3")]
        [Example(Description = "Adddress Line 3")]
        public string Address3 { get; set; }

        [RegularExpression(AddressRegEx)]
        [Display(Description = "Customer home address line 4")]
        [Example(Description = "Adddress Line 4")]
        public string Address4 { get; set; }

        [RegularExpression(AddressRegEx)]
        [Display(Description = "Customer home address line 5")]
        [Example(Description = "Adddress Line 5")]
        public string Address5 { get; set; }

        [Required]
        [StringLength(10)]
        [RegularExpression(PostcodeRegEx, ErrorMessage = "Please enter a valid postcode")]
        [Display(Description = "Customers postcode within England.")]
        [Example(Description = "AA11AA")]
        public string PostCode { get; set; }

        [StringLength(10)]
        [RegularExpression(PostcodeRegEx)]
        [Display(Description = "This should be used where the customers home address is not within England.")]
        [Example(Description = "AA11AA")]
        public string AlternativePostCode { get; set; }

        [RegularExpression(@"^(\+|-)?(?:180(?:(?:\.0{1,6})?)|(?:[0-9]|[1-9][0-9]|1[0-7][0-9])(?:(?:\.[0-9]{1,6})?))$")]
        [Display(Description = "Geocoded address information")]
        [Example(Description = "-1.50812")]
        public decimal? Longitude { get; set; }

        [RegularExpression(@"^(\+|-)?(?:90(?:(?:\.0{1,6})?)|(?:[0-9]|[1-8][0-9])(?:(?:\.[0-9]{1,6})?))$")]
        [Display(Description = "Geocoded address information")]
        [Example(Description = "52.40100")]
        public decimal? Latitude { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date the customer started residing at this location")]
        [Example(Description = "2018-06-19T09:01:00")]
        public DateTime? EffectiveFrom { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date the customer end residence at this location")]
        [Example(Description = "2018-06-21T13:12:00")]
        public DateTime? EffectiveTo { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time of the last modification to the record.")]
        [Example(Description = "2018-06-21T13:45:00")]
        public DateTime? LastModifiedDate { get; set; }

        [StringLength(10, MinimumLength = 10)]
        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        [Example(Description = "0000000001")]
        public string LastModifiedTouchpointId { get; set; }
        
        [StringLength(50)]
        [Display(Description = "Identifier supplied by the touchpoint to indicate their subcontractor")]
        [Example(Description = "01234567899876543210")]
        public string SubcontractorId { get; set; }

        [JsonIgnoreOnSerialize]
        public string CreatedBy { get; set; }

        public void SetDefaultValues()
        {
            var addressId = Guid.NewGuid();
            AddressId = addressId;

            if (!LastModifiedDate.HasValue)
                LastModifiedDate = DateTime.UtcNow;
        }

        public void SetIds(Guid customerId, string touchpointId, string subcontractorId)
        {
            AddressId = Guid.NewGuid();
            CustomerId = customerId;
            LastModifiedTouchpointId = touchpointId;
            SubcontractorId = subcontractorId;
            CreatedBy = touchpointId;
        }
    }
}