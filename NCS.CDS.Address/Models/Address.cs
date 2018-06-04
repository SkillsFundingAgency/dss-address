using System;
using System.ComponentModel.DataAnnotations;

namespace NCS.CDS.Address.Models
{
    public class Address
    {
        private const string AddressRegEx = @"[A-Za-z0-9 ~!@&amp;'\()*+,\-.\/:;]{1,100}";
        private const string PostcodeRegEx = @"([Gg][Ii][Rr] 0[Aa]{2})|((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A - Za - z][0 - 9][A - Za - z]) | ([A - Za - z][A - Ha - hJ - Yj - y][0 - 9]?[A - Za - z]))))\\s?[0-9] [A-Za-z]{2})";

        public Guid AddressId { get; set; }
        public Guid CustomerId { get; set; }

        [StringLength(100)]
        [RegularExpression(AddressRegEx)]
        public string Address1 { get; set; }

        [StringLength(100)]
        [RegularExpression(AddressRegEx)]
        public string Address2 { get; set; }

        [StringLength(100)]
        [RegularExpression(AddressRegEx)]
        public string Address3 { get; set; }

        [StringLength(100)]
        [RegularExpression(AddressRegEx)]
        public string Address4 { get; set; }

        [StringLength(100)]
        [RegularExpression(AddressRegEx)]
        public string Address5 { get; set; }

        [StringLength(10)]
        [RegularExpression(PostcodeRegEx)]
        public string PostCode { get; set; }

        [StringLength(10)]
        [RegularExpression(PostcodeRegEx)]
        public string AlternativePostCode { get; set; }

        public decimal Longitude { get; set; }
        public decimal Latitude { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime EffectiveFrom { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime EffectiveTo { get; set; }

        public Guid LastModifiedTouchpointId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime LastModifiedDate { get; set; }
    }
}