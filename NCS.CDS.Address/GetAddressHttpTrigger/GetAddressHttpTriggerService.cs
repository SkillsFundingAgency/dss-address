using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCS.CDS.Address.GetAddressHttpTrigger
{
    public class GetAddressHttpTriggerService
    {
        public async Task<List<Models.Address>> GetAddresses()
        {
            var result = CreateTempAddresses();
            return await Task.FromResult(result);
        }

        public List<Models.Address> CreateTempAddresses()
        {
            var addressList = new List<Models.Address>
            {
                new Models.Address
                {
                    AddressId = Guid.Parse("187de7d1-292f-43e8-b936-f28f65857c24"),
                    Address1 = "Cheylesmore House",
                    Address2 = "5 Quinton Rd",
                    Address3 = "Coventry",
                    PostCode = "CV1 2WT",
                    Latitude = (decimal) 52.400997,
                    Longitude = (decimal) -1.508122,
                    EffectiveFrom = DateTime.Today,
                    EffectiveTo = DateTime.Today.AddYears(20)
                },
                new Models.Address
                {
                    AddressId = Guid.Parse("cc8cf9f0-68e5-489b-91a2-7b9978fc0605"),
                    Address1 = "The Daily Bugle",
                    Address2 = "175 Fifth Ave",
                    Address3 = "Flatiron District",
                    Address4 = "New York",
                    PostCode = "NY 10010",
                    Latitude = (decimal) 52.400997,
                    Longitude = (decimal) -1.508122,
                    EffectiveFrom = DateTime.Today,
                    EffectiveTo = DateTime.Today.AddYears(50)
                },
                new Models.Address
                {
                    AddressId = Guid.Parse("2ee14c94-5a4a-425a-85d5-a579591b29d8"),
                    Address1 = "Avengers Mansion",
                    Address2 = "890 Fifth Ave",
                    Address3 = "Upper East Side",
                    Address4 = "New York",
                    PostCode = "NY 10065",
                    Latitude = (decimal) 40.738832,
                    Longitude = (decimal) -73.981534,
                    EffectiveFrom = DateTime.Today,
                    EffectiveTo = DateTime.Today.AddYears(100)
                }
            };

            return addressList;
        }

    }
}
