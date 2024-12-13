using Azure.Messaging.ServiceBus;
using DFC.GeoCoding.Standard.AzureMaps.Service;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.Address.Cosmos.Helper;
using NCS.DSS.Address.Cosmos.Provider;
using NCS.DSS.Address.GeoCoding;
using NCS.DSS.Address.GetAddressByIdHttpTrigger.Service;
using NCS.DSS.Address.GetAddressHttpTrigger.Service;
using NCS.DSS.Address.Helpers;
using NCS.DSS.Address.Models;
using NCS.DSS.Address.PatchAddressHttpTrigger.Service;
using NCS.DSS.Address.PostAddressHttpTrigger.Service;
using NCS.DSS.Address.ServiceBus;
using NCS.DSS.Address.Validation;

namespace NCS.DSS.Address
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWebApplication()
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;
                    services.AddOptions<AddressConfigurationSettings>()
                        .Bind(configuration);

                    services.AddApplicationInsightsTelemetryWorkerService();
                    services.ConfigureFunctionsApplicationInsights();
                    services.AddSingleton<IResourceHelper, ResourceHelper>();
                    services.AddSingleton<IValidate, Validate>();
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
                    services.AddTransient<ICosmosDbProvider, CosmosDbProvider>();
                    services.AddTransient<IAddressServiceBusClient, AddressServiceBusClient>();
                    services.AddLogging();

                    services.AddSingleton(s =>
                    {
                        var settings = s.GetRequiredService<IOptions<AddressConfigurationSettings>>().Value;
                        var options = new CosmosClientOptions() { ConnectionMode = ConnectionMode.Gateway };

                        return new CosmosClient(settings.AddressConnectionString, options);
                    });

                    services.AddSingleton(s =>
                    {
                        var settings = s.GetRequiredService<IOptions<AddressConfigurationSettings>>().Value;

                        return new ServiceBusClient(settings.ServiceBusConnectionString);
                    });

                    services.Configure<LoggerFilterOptions>(options =>
                    {
                        LoggerFilterRule toRemove = options.Rules.FirstOrDefault(rule => rule.ProviderName
                            == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
                        if (toRemove is not null)
                        {
                            options.Rules.Remove(toRemove);
                        }
                    });
                })
                .Build();
            await host.RunAsync();
        }
    }
}