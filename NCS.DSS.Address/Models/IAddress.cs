using System;

namespace NCS.DSS.Address.Models
{
    public interface IAddress
    {
        string Address1 { get; set; }
        string Address2 { get; set; }
        string Address3 { get; set; }
        string Address4 { get; set; }
        string Address5 { get; set; }
        string PostCode { get; set; }
        string AlternativePostCode { get; set; }
        decimal? Longitude { get; set; }
        decimal? Latitude { get; set; }
        DateTime? EffectiveFrom { get; set; }
        DateTime? EffectiveTo { get; set; }
        DateTime? LastModifiedDate { get; set; }
        string LastModifiedTouchpointId { get; set; }

        void SetDefaultValues();

    }
}