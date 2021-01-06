using DFC.Common.Standard.Logging;
using DFC.GeoCoding.Standard.AzureMaps.Service;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.Address;
using NCS.DSS.Address.Cosmos.Helper;
using NCS.DSS.Address.Cosmos.Provider;
using NCS.DSS.Address.GeoCoding;
using NCS.DSS.Address.GetAddressByIdHttpTrigger.Service;
using NCS.DSS.Address.GetAddressHttpTrigger.Service;
using NCS.DSS.Address.PatchAddressHttpTrigger.Service;
using NCS.DSS.Address.PostAddressHttpTrigger.Service;
using NCS.DSS.Address.Validation;

[assembly: FunctionsStartup(typeof(Startup))]
namespace NCS.DSS.Address
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<IResourceHelper, ResourceHelper>();
            builder.Services.AddSingleton<IValidate, Validate>();
            builder.Services.AddSingleton<ILoggerHelper, LoggerHelper>();
            builder.Services.AddSingleton<IHttpRequestHelper, HttpRequestHelper>();
            builder.Services.AddSingleton<IHttpResponseMessageHelper, HttpResponseMessageHelper>();
            builder.Services.AddSingleton<IJsonHelper, JsonHelper>();
            builder.Services.AddScoped<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
            builder.Services.AddTransient<IGetAddressHttpTriggerService, GetAddressHttpTriggerService>();
            builder.Services.AddTransient<IGetAddressByIdHttpTriggerService, GetAddressByIdHttpTriggerService>();
            builder.Services.AddTransient<IPostAddressHttpTriggerService, PostAddressHttpTriggerService>();
            builder.Services.AddTransient<IPatchAddressHttpTriggerService, PatchAddressHttpTriggerService>();
            builder.Services.AddScoped<IAddressPatchService, AddressPatchService>();
            builder.Services.AddScoped<IGeoCodingService, GeoCodingService>();
            builder.Services.AddScoped<IAzureMapService, AzureMapService>();
            builder.Services.AddTransient<IDocumentDBProvider, DocumentDBProvider>();
            builder.Services.AddLogging();

        }
    }
}
