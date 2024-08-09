using DFC.Common.Standard.Logging;
using DFC.GeoCoding.Standard.AzureMaps.Service;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCS.DSS.Address.Cosmos.Helper;
using NCS.DSS.Address.Cosmos.Provider;
using NCS.DSS.Address.GeoCoding;
using NCS.DSS.Address.GetAddressByIdHttpTrigger.Service;
using NCS.DSS.Address.GetAddressHttpTrigger.Service;
using NCS.DSS.Address.Helpers;
using NCS.DSS.Address.PatchAddressHttpTrigger.Service;
using NCS.DSS.Address.PostAddressHttpTrigger.Service;
using NCS.DSS.Address.Validation;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddSingleton<IResourceHelper, ResourceHelper>();
        services.AddSingleton<IValidate, Validate>();
        services.AddSingleton<ILoggerHelper, LoggerHelper>();
        services.AddSingleton<IHttpRequestHelper, HttpRequestHelper>();
        services.AddSingleton<IHttpResponseMessageHelper, HttpResponseMessageHelper>();
        services.AddSingleton<IJsonHelper, JsonHelper>();
        services.AddSingleton<IDynamicHelper, DynamicHelper>();
        services.AddScoped<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
        services.AddTransient<IGetAddressHttpTriggerService, GetAddressHttpTriggerService>();
        services.AddTransient<IGetAddressByIdHttpTriggerService, GetAddressByIdHttpTriggerService>();
        services.AddTransient<IPostAddressHttpTriggerService, PostAddressHttpTriggerService>();
        services.AddTransient<IPatchAddressHttpTriggerService, PatchAddressHttpTriggerService>();
        services.AddScoped<IAddressPatchService, AddressPatchService>();
        services.AddScoped<IGeoCodingService, GeoCodingService>();
        services.AddScoped<IAzureMapService, AzureMapService>();
        services.AddTransient<IDocumentDBProvider, DocumentDBProvider>();
        services.AddLogging();
    })
    .Build();

host.Run();
