using DFC.Common.Standard.Logging;
using DFC.Functions.DI.Standard;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.Address.Cosmos.Helper;
using NCS.DSS.Address.Cosmos.Provider;
using NCS.DSS.Address.GetAddressByIdHttpTrigger.Service;
using NCS.DSS.Address.GetAddressHttpTrigger.Service;
using NCS.DSS.Address.Ioc;
using NCS.DSS.Address.PatchAddressHttpTrigger.Service;
using NCS.DSS.Address.PostAddressHttpTrigger.Service;
using NCS.DSS.Address.Validation;

[assembly: WebJobsStartup(typeof(WebJobsExtensionStartup), "Web Jobs Extension Startup")]

namespace NCS.DSS.Address.Ioc
{
    public class WebJobsExtensionStartup : IWebJobsStartup
    {

        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddDependencyInjection();

            RegisterHelpers(builder);
            RegisterServices(builder);
            RegisterDataProviders(builder);
        }

        private void RegisterHelpers(IWebJobsBuilder builder)
        {
            builder.Services.AddSingleton<IResourceHelper, ResourceHelper>();
            builder.Services.AddSingleton<IValidate, Validate>();
            builder.Services.AddSingleton<ILoggerHelper, LoggerHelper>();
            builder.Services.AddSingleton<IHttpRequestHelper, HttpRequestHelper>();
            builder.Services.AddSingleton<IHttpResponseMessageHelper, HttpResponseMessageHelper>();
            builder.Services.AddSingleton<IJsonHelper, JsonHelper>();
            builder.Services.AddScoped<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
        }

        private void RegisterServices(IWebJobsBuilder builder)
        {
            builder.Services.AddTransient<IGetAddressHttpTriggerService, GetAddressHttpTriggerService>();
            builder.Services.AddTransient<IGetAddressByIdHttpTriggerService, GetAddressByIdHttpTriggerService>();
            builder.Services.AddTransient<IPostAddressHttpTriggerService, PostAddressHttpTriggerService>();
            builder.Services.AddTransient<IPatchAddressHttpTriggerService, PatchAddressHttpTriggerService>();
            builder.Services.AddScoped<IAddressPatchService, AddressPatchService>();
        }

        private void RegisterDataProviders(IWebJobsBuilder builder)
        {
            builder.Services.AddSingleton<IDocumentDBProvider, DocumentDBProvider>();
        }

    }
}
