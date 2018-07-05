using System;
using Microsoft.Azure.WebJobs.Description;

namespace NCS.DSS.Address.Ioc
{
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class InjectAttribute : Attribute
    {
    }
}
