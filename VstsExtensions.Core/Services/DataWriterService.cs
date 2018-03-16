using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using VstsExtensions.Core.Helpers;
using VstsExtensions.Core.Models;

namespace VstsExtensions.Core.Services
{
    public class DataWriterService : IDataWriterService
    {
        public IOrganizationService Service { get; set; }
        public IFileManager FileManager { get; set; }

        private ExportedData ExportedData { get; set; }

        private enum OperationType { Create, Update, Associate };

        public DataWriterService(IOrganizationService service, IFileManager fileManager)
        {
            Service = service ?? throw new ArgumentNullException(nameof(service));
            ExportedData = new ExportedData();
            FileManager = fileManager ?? new FileManager();
        }

        public void ImportDataToCrm(List<Entity> ec, Version crmVersion, Dictionary<Guid, Guid> guidMap, bool manyToMany, bool createOnly, bool updateOnly)
        {
            var isLegacyCrmVersion = IsLegacyCrmVersion(crmVersion);
            var importOperation = new OperationType();
            
            foreach (var entity in ec)
            {
                var isUpdateSuccessful = false;
                var isCreateSuccessful = false;

                try
                {
                    MapEntityGuids(entity, guidMap);

                    if (manyToMany)
                    {
                        importOperation = OperationType.Associate;
                        AssociateManyToMany(entity);
                    }
                    else
                    {
                        if (createOnly)
                        {
                            importOperation = OperationType.Create;
                            var entityId = CreateEntity(entity);

                            isCreateSuccessful = entityId != Guid.Empty;
                        }
                        else
                        {
                            isUpdateSuccessful = TryUpdateEntity(entity, isLegacyCrmVersion, updateOnly, out importOperation);
                        }
                        
                        if (isCreateSuccessful || isUpdateSuccessful)
                        {
                            if (isLegacyCrmVersion && importOperation == OperationType.Update)
                            {
                                AssignEntity(entity);
                            }

                            SetEntityState(entity);  
                        }
                    }
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    var operation = (importOperation == OperationType.Create) ? "CREATE" : "UPDATE";
                    Console.WriteLine(string.Format("RECORD ERROR: {0}, {1}, OPERATION: {2}, MESSAGE: {3}", entity.Id, entity.LogicalName, operation, ex.Detail?.Message));
                }
            }
        }

        public void SetEntityState(Entity entity)
        {
            var state = entity.GetAttributeValue<OptionSetValue>("statecode")?.Value ?? -1;
            var status = entity.GetAttributeValue<OptionSetValue>("statuscode")?.Value ?? -1;

            if (state != -1 && status != -1)
            {
                try
                {
                    var request = new SetStateRequest
                    {
                        State = new OptionSetValue(state),
                        Status = new OptionSetValue(status),
                        EntityMoniker = new EntityReference(entity.LogicalName, entity.Id)
                    };

                    Service.Execute(request);
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new FaultException<OrganizationServiceFault>(ex.Detail);
                }
            }
        }

        public void MapEntityGuids(Entity entity, Dictionary<Guid, Guid> guidMap)
        {
            var guidsToUpdate = new List<KeyValuePair<string, object>>();

            entity.Attributes.Where(a => a.Value is EntityReference).ToList().ForEach(a =>
            {
                var source = ((EntityReference)a.Value);
                var guidIsMapped = guidMap.TryGetValue(source.Id, out var targetId);

                if (guidIsMapped)
                {
                    source.Id = targetId;
                    guidsToUpdate.Add(new KeyValuePair<string, object>(a.Key, source));
                }
            });

            foreach (var attribute in guidsToUpdate)
            {
                entity[attribute.Key] = attribute.Value;
            }
        }

        private void AssignEntity(Entity entity)
        {
            if (entity.Attributes.Contains("ownerid"))
            {
                var assign = new AssignRequest
                {
                    Assignee = (EntityReference)entity["ownerid"],
                    Target = new EntityReference(entity.LogicalName, entity.Id)
                };
                try
                {
                    Service.Execute(assign);
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new FaultException<OrganizationServiceFault>(ex.Detail);
                }
            }
        }

        private Guid CreateEntity(Entity entity)
        {
            RemoveStatusAttributes(entity);
            return Service.Create(entity);
        }

        private bool TryUpdateEntity(Entity entity, bool isLegacyCrmVersion, bool updateOnly, out OperationType importOperation)
        {
            var isUpdateSuccessful = false;
            importOperation = OperationType.Update;

            try
            {
                // If version is below 7.1 then remove statecode and statuscode attributes from entity.
                if (isLegacyCrmVersion)
                {
                    RemoveStatusAttributes(entity);
                }


                Service.Update(entity);
                isUpdateSuccessful = true;
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                if (!updateOnly)
                {
                    RemoveStatusAttributes(entity);

                    // Try the create step if the update failed because the record doesn't already exist to update.
                    if (ex.Message.ToUpper().EndsWith("DOES NOT EXIST") || ex.Message.ToUpper().Contains("NO OBJECT MATCHED THE QUERY"))
                    {
                        importOperation = OperationType.Create;
                        Service.Create(entity);
                        isUpdateSuccessful = true;
                    }
                    else
                    {
                        Console.WriteLine($"Critical error trying to create entity with id {entity.Id.ToString()}. Message: {ex.Detail}");
                    }
                }
                else
                {
                    isUpdateSuccessful = false;
                    Console.WriteLine($"Exception thrown updating entity with id {entity.Id.ToString()}. Message: {ex.Detail}");
                }
            }

            return isUpdateSuccessful;
        }

        public void RemoveStatusAttributes(Entity entity)
        {
            var statusAttributes = new string[] { "statecode", "statuscode" };

            foreach (var attribute in statusAttributes)
            {
                if (entity.Contains(attribute))
                {
                    entity.Attributes.Remove(attribute);
                }
            }
        }

        public void AssociateManyToMany(Entity entity)
        {
            // Record id attribute can't be used for create/update operations.            
            entity.Attributes.Remove(entity.LogicalName + "id");

            var attributes = entity.Attributes.Where(a => a.Value is EntityReference).ToList();

            // N:N association should only be carried out if 2 attributes are present.
            if (attributes.Count == 2)
            {
                // The related records are stored as guids, so we have to figure out the entity logical names by removing the "id" from the end of the attribute name.
                // The "right" way to do this would be a metadata call to retrieve the relationship details, but this should work and the performance is better.
                var entity1LogicalName = StringHelper.ReplaceLastOccurrence(attributes[0].Key, "id", "");
                var entity2LogicalName = StringHelper.ReplaceLastOccurrence(attributes[1].Key, "id", "");

                // Set the second attribute to be the related entity in the associate call
                var related = new EntityReferenceCollection();
                related.Add(new EntityReference { Id = ((EntityReference)attributes[1].Value).Id , LogicalName = entity2LogicalName });

                var relationship = new Relationship(entity.LogicalName);
                try
                {
                    Service.Associate(entity1LogicalName, ((EntityReference)attributes[0].Value).Id, relationship, related);
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    if (ex.Message.ToUpper().Contains("CANNOT INSERT DUPLICATE KEY"))
                    {
                        Console.WriteLine($"Tried to associate {entity1LogicalName} with {entity2LogicalName} but association already existed. Continuing.");
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else
            {
                Console.WriteLine("Found more than two GUID attributes, exiting associate n:n step.");
            }
        }

        public void WriteDataToFile(ExportedData data, string directory)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.None, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.None,
                Formatting = Formatting.None
            });

            FileManager.StoreJsonConfig(json, "ExportedData", directory);
        }
        
        private bool IsLegacyCrmVersion(Version crmVersion)
        {
            return crmVersion.Major < 7 || (crmVersion.Major == 7 && crmVersion.Minor < 1);
        }
    }
}