using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VstsExtensions.Core.Helpers;
using VstsExtensions.Core.Services;

namespace VstsExtensions.Core.Models
{
    public class DataTransferService : IDataTransferService
    {
        public IDataWriterService DataWriter { get; set; }
        public IDataReaderService DataReader { get; set; }
        public IDataTransformService DataTransformer { get; set; }
        public IFileManager FileManager { get; set; }

        private IOrganizationService Service { get; }

        public DataTransferService(IOrganizationService service)
        {
            Service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public void SetDefaultProperties()
        {
            FileManager = FileManager ?? new FileManager();
            DataWriter = DataWriter ?? new DataWriterService(Service, FileManager);
            DataReader = DataReader ?? new DataReaderService(Service, FileManager);
            DataTransformer = DataTransformer ?? new DataTransformService();
        }
    }
}
