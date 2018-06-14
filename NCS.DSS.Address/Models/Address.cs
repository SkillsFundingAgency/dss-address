using System;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Address.Models
{
    public class Address
    {
        private const string AddressRegEx = @"[A-Za-z0-9 ~!@&amp;'\()*+,\-.\/:;]{1,100}";
        private const string PostcodeRegEx = @"([Gg][Ii][Rr] 0[Aa]{2})|((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A - Za - z][0 - 9][A - Za - z]) | ([A - Za - z][A - Ha - hJ - Yj - y][0 - 9]?[A - Za - z]))))\\s?[0-9] [A-Za-z]{2})";

        [Display(Description = "Unique identifier for an address.")]
        public Guid AddressId { get; set; }

        [Required]
        [Display(Description = "Unique identifier of a customer")]
        public Guid CustomerId { get; set; }

        [Required]
        [StringLength(100)]
        [RegularExpression(AddressRegEx)]
        [Display(Description = "Customer home address line 1")]
        public string Address1 { get; set; }

        [StringLength(100)]
        [RegularExpression(AddressRegEx)]
        [Display(Description = "Customer home address line 2")]
        public string Address2 { get; set; }

        [StringLength(100)]
        [RegularExpression(AddressRegEx)]
        [Display(Description = "Customer home address line 3")]
        public string Address3 { get; set; }

        [StringLength(100)]
        [RegularExpression(AddressRegEx)]
        [Display(Description = "Customer home address line 4")]
        public string Address4 { get; set; }

        [StringLength(100)]
        [RegularExpression(AddressRegEx)]
        [Display(Description = "Customer home address line 5")]
        public string Address5 { get; set; }

        [Required]
        [StringLength(10)]
        [RegularExpression(PostcodeRegEx, ErrorMessage = "Please enter a valid postcode")]
        [Display(Description = "Customers postcode within England.")]
        public string PostCode { get; set; }

        [StringLength(10)]
        [RegularExpression(PostcodeRegEx)]
        [Display(Description = "This should be used where the customers home address is not within England.")]
        public string AlternativePostCode { get; set; }

        [Display(Description = "Geocoded address information")]
        public decimal Longitude { get; set; }

        [Display(Description = "Geocoded address information")]
        public decimal Latitude { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date the customer started residing at this location")]
        public DateTime EffectiveFrom { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date the customer end residence at this location")]
        public DateTime EffectiveTo { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time of the last modification to the record.")]
        public DateTime LastModifiedDate { get; set; }

        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        public Guid LastModifiedTouchpointId { get; set; }
    }
}