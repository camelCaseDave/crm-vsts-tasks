using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using VstsExtensions.Core.Models;

namespace VstsExtensions.Core.Services
{
    public interface IDataTransformService
    {
        List<Entity> TransformDataForCrm(ExportedData exportedData);
        ExportedData TransformDataForFile(List<Entity> entities);
    }
}