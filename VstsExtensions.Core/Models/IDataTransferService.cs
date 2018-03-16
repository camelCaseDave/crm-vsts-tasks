using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VstsExtensions.Core.Helpers;
using VstsExtensions.Core.Services;

namespace VstsExtensions.Core.Models
{
    public interface IDataTransferService
    {
        IDataWriterService DataWriter { get; set; }
        IDataReaderService DataReader { get; set; }
        IDataTransformService DataTransformer { get; set; }
        IFileManager FileManager { get; set; }

        void SetDefaultProperties();
    }
}
