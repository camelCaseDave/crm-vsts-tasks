using VstsExtensions.Core;
using VstsExtensions.Core.Models;
using VstsExtensions.Core.Services;
using System;
using System.Collections.Generic;

namespace VstsExtensions
{
    public class ImportToCrm
    {
        public string WorkingDirectory { get; }
        public IXrmCommand XrmCommand { get; set; }
        public IDataTransferService DataTransferService { get; set; }

        public ImportToCrm(string connectionString, string workingDirectory)
        {
            WorkingDirectory = workingDirectory;
            XrmCommand = new XrmCommand(connectionString);
        }

        public void Run()
        {
            var service = XrmCommand.GetService();
            DataTransferService = new DataTransferService(service);
            DataTransferService.SetDefaultProperties();

            // TODO, read guid map from user input.
            var guidMap = new Dictionary<Guid, Guid>();
            var data = DataTransferService.DataReader.ParseDataFromFile(WorkingDirectory);
            var parsedData = DataTransferService.DataTransformer.TransformDataForCrm(data);

            var version = DataTransferService.DataReader.GetCrmVersion();

            DataTransferService.DataWriter.ImportDataToCrm(parsedData, version, guidMap, false, false, false);           
        }
    }
}