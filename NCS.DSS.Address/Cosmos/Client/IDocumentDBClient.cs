using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Address.Cosmos.Client
{
    public interface IDocumentDBClient
    {
        DocumentClient CreateDocumentClient();
    }
}