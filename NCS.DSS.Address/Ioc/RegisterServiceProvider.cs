using System;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.Address.Cosmos.Helper;
using NCS.DSS.Address.Cosmos.Provider;
using NCS.DSS.Address.GetAddressByIdHttpTrigger.Service;
using NCS.DSS.Address.GetAddressHttpTrigger.Service;
using NCS.DSS.Address.Helpers;
using NCS.DSS.Address.PatchAddressHttpTrigger.Service;
using NCS.DSS.Address.PostAddressHttpTrigger.Service;
using NCS.DSS.Address.Validation;


namespace NCS.DSS.Address.Ioc
{
    public class RegisterServiceProvider
    {
        public IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddTransient<IGetAddressHttpTriggerService, GetAddressHttpTriggerService>();
            services.AddTransient<IGetAddressByIdHttpTriggerService, GetAddressByIdHttpTriggerService>();
            services.AddTransient<IPostAddressHttpTriggerService, PostAddressHttpTriggerService>();
            services.AddTransient<IPatchAddressHttpTriggerService, PatchAddressHttpTriggerService>();
            services.AddTransient<IAddressPatchService, AddressPatchService>();

            services.AddTransient<IResourceHelper, ResourceHelper>();
            services.AddTransient<IValidate, Validate>();
            services.AddTransient<IHttpRequestMessageHelper, HttpRequestMessageHelper>();
            services.AddTransient<IDocumentDBProvider, DocumentDBProvider>();

            return services.BuildServiceProvider(true);
        }
    }
}
