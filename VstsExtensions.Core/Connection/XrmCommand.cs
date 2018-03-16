using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Net;

namespace VstsExtensions.Core
{
    public class XrmCommand : IXrmCommand
    {
        private string ConnectionString { get; }
        private IOrganizationService Service { get; set; }

        public XrmCommand(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public XrmCommand(IOrganizationService service)
        {
            Service = service;
        }

        public IOrganizationService GetService(bool refresh = false)
        {
            if (Service == null || (Service != null && refresh))
            {
                Console.WriteLine($"Connecting to CRM.");

                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                Service = new CrmServiceClient(ConnectionString);

                var response = (WhoAmIResponse)Service?.Execute(new WhoAmIRequest());
                Console.WriteLine($"Executing WhoAmIRequest. User Id is: {response?.UserId}");

                if (response?.UserId == null)
                {
                    throw new InvalidPluginExecutionException($"Unable to connect to CRM. Check your connection string.\n {((CrmServiceClient)Service).LastCrmError}.");
                }
            }

            return Service;
        }
    }
}