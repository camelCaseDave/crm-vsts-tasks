using Microsoft.Xrm.Sdk;
using VstsExtensions.Core;
using VstsExtensions.Core.Helpers;
using VstsExtensions.Core.Models;
using VstsExtensions.Core.Services;

namespace VstsExtensions
{
    public class ExportToFile
    {
        public string WorkingDirectory { get; }
        public string FetchXml { get; }
        public IXrmCommand XrmCommand { get; set; }
        public IDataTransferService DataTransferService { get; set; }

        public ExportToFile(string connectionString, string workingDirectory, string fetchXml)
        {
            WorkingDirectory = workingDirectory;
            FetchXml = fetchXml;
            XrmCommand = new XrmCommand(connectionString);
        }        

        public void Run()
        {
            var service = XrmCommand.GetService();
            DataTransferService = new DataTransferService(service);
            DataTransferService.SetDefaultProperties();

            var data = DataTransferService.DataReader.GetDataFromCrm(FetchXml);
            var parsedData = DataTransferService.DataTransformer.TransformDataForFile(data);

            DataTransferService.DataWriter.WriteDataToFile(parsedData, WorkingDirectory);
        }        
    }
}