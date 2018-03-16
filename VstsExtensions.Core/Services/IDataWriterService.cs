using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using VstsExtensions.Core.Helpers;
using VstsExtensions.Core.Models;

namespace VstsExtensions.Core.Services
{
    public interface IDataWriterService
    {
        IOrganizationService Service { get; set; }
        IFileManager FileManager { get; set; }

        void ImportDataToCrm(List<Entity> ec, Version crmVersion, Dictionary<Guid, Guid> guidMap, bool manyToMany, bool createOnly, bool updateOnly);
        void WriteDataToFile(ExportedData data, string directory);
        void RemoveStatusAttributes(Entity entity);
        void AssociateManyToMany(Entity entity);
        void MapEntityGuids(Entity entity, Dictionary<Guid, Guid> guidMap);
        void SetEntityState(Entity entity);
    }
}