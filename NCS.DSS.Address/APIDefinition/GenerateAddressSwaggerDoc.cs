using DFC.Swagger.Standard;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Mvc;

namespace NCS.DSS.Address.APIDefinition
{
    public class GenerateAddressSwaggerDoc
    {
        public const string ApiTitle = "Addresses";
        public const string ApiDefinitionName = "API-Definition";
        public const string ApiDefRoute = ApiTitle + "/" + ApiDefinitionName;
        public const string ApiDescription = "To support the Data Collections integration with DSS SubcontractorId has been added as an attribute. " +
                                             "Geocoding has also been added to the PostCode attribute.";
        public const string ApiVersion = "3.0.0";
        private ISwaggerDocumentGenerator _swaggerDocumentGenerator;

        public GenerateAddressSwaggerDoc(ISwaggerDocumentGenerator swaggerDocumentGenerator)
        {
            _swaggerDocumentGenerator = swaggerDocumentGenerator;
        }

        [Function(ApiDefinitionName)]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = ApiDefRoute)]HttpRequest req)
        {
            var swagger = _swaggerDocumentGenerator.GenerateSwaggerDocument(req, ApiTitle, ApiDescription,
                ApiDefinitionName, ApiVersion, Assembly.GetExecutingAssembly());

            if (string.IsNullOrEmpty(swagger))
                return new NoContentResult();

            return new OkObjectResult(swagger);
        }
    }
}