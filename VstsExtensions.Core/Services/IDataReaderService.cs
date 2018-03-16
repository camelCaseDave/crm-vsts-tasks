using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using VstsExtensions.Core.Helpers;
using VstsExtensions.Core.Models;

namespace VstsExtensions.Core.Services
{
    public interface IDataReaderService
    {
        IOrganizationService Service { get; set; }
        IFileManager FileManager { get; set; }

        List<Entity> GetDataFromCrm(string fetchXml);

        ExportedData ParseDataFromFile(string directory);
        Version GetCrmVersion();
    }
}