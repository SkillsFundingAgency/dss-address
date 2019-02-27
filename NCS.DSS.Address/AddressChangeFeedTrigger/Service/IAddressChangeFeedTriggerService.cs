using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace NCS.DSS.Address.AddressChangeFeedTrigger.Service
{
    public interface IAddressChangeFeedTriggerService
    {
        Task SendMessageToChangeFeedQueueAsync(Document document);
    }
}
