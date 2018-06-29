using System;

namespace NCS.DSS.Address.PostAddressHttpTrigger
{
    public class PostAddressHttpTriggerService
    {
        public Guid? Create(Models.Address address)
        {
            if (address == null)
                return null;

            return Guid.NewGuid();
        }
    }
}