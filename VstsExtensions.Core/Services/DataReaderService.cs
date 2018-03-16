using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using VstsExtensions.Core.Helpers;
using VstsExtensions.Core.Models;

namespace VstsExtensions.Core.Services
{
    public class DataReaderService : IDataReaderService
    {
        public IOrganizationService Service { get; set; }
        public IFileManager FileManager { get; set; }
        
        public DataReaderService(IOrganizationService service, IFileManager fileManager)
        {
            Service = service ?? throw new ArgumentNullException(nameof(service));
            FileManager = fileManager ?? new FileManager();
        }

        public List<Entity> GetDataFromCrm(string fetchXml)
        {
            var ec = new List<Entity>();

            var fetchCount = 5000;
            var pageNumber = 1;
            string pagingCookie = null;

            while (true)
            {
                var xml = XmlParser.CreateXml(fetchXml, pagingCookie, pageNumber, fetchCount);

                var retrieved = Service.RetrieveMultiple(new FetchExpression(xml));
                ec.AddRange(retrieved.Entities);

                if (retrieved.MoreRecords)
                {
                    pageNumber++;
                    pagingCookie = retrieved.PagingCookie;
                }
                else
                {
                    break;
                }
            }

            return ec;
        }

        public ExportedData ParseDataFromFile(string directory)
        {
            if (directory.ToUpper().StartsWith("FILE="))
            {
                directory = Regex.Replace(directory, "FILE=", "", RegexOptions.IgnoreCase);
            }

            var json = FileManager.GetJsonConfig("ExportedData", directory);
            var exportedData = JsonConvert.DeserializeObject<ExportedData>(json);

            return exportedData;
        }

        public Version GetCrmVersion()
        {
            if (Service is CrmServiceClient client)
            {
                return client.ConnectedOrgVersion;
            }
            else
            {
                var request = new RetrieveVersionRequest();
                var response = (RetrieveVersionResponse)Service.Execute(request);

                return new Version(response.Version);
            }
        }
    }
}