using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using VstsExtensions.Core.Models;

namespace VstsExtensions.Core.Services
{
    public class DataTransformService : IDataTransformService
    {
        public ExportedData TransformDataForFile(List<Entity> entities)
        {
            var parsedData = TransformEntityList(entities);
            return new ExportedData
            {
                RecordSets = parsedData
            };
        }
        
        public List<Entity> TransformDataForCrm(ExportedData exportedData)
        {
            var data = exportedData.RecordSets;

            return TransformExportedEntityList(data);
        }

        private List<ExportEntity> TransformEntityList(List<Entity> entitylist)
        {
            var entitiesToExport = new List<ExportEntity>();
            foreach (Entity e in entitylist)
            {
                var exportEntity = new ExportEntity
                {
                    Id = e.Id,
                    LogicalName = e.LogicalName
                };

                foreach (var attribute in e.Attributes)
                {
                    // Leave out the entity id and logical name from the attribute collection .
                    if ((attribute.Key.ToUpper() != e.LogicalName.ToUpper() + "ID")
                        && (attribute.Key.ToUpper() != "LOGICALNAME"))
                    {
                        var exportAttribute = new ExportAttribute
                        {
                            AttributeName = attribute.Key,
                            AttributeType = attribute.Value.GetType().ToString()
                        };

                        if (exportAttribute.AttributeType == "Microsoft.Xrm.Sdk.EntityCollection")
                        {
                            var ec = (EntityCollection)attribute.Value;
                            var entities = new List<Entity>();
                            foreach (var entity in ec.Entities)
                            {
                                entities.Add(entity);
                            }
                            exportAttribute.AttributeValue = TransformEntityList(entities);
                        }
                        else
                        {
                            exportAttribute.AttributeValue = attribute.Value;
                        }
                        exportEntity.Attributes.Add(exportAttribute);
                    }
                }

                entitiesToExport.Add(exportEntity);
            }

            return entitiesToExport;
        }

        private List<Entity> TransformExportedEntityList(List<ExportEntity> entityList)
        {
            var ec = new List<Entity>();
            foreach (var e in entityList)
            {
                var entity = new Entity
                {
                    Id = e.Id,
                    LogicalName = e.LogicalName
                };

                foreach (var exportAttribute in e.Attributes)
                {
                    var attributeName = exportAttribute.AttributeName;
                    var attributeValue = TransformAttribute(exportAttribute, entity);

                    entity.Attributes.Add(attributeName, attributeValue);
                }

                ec.Add(entity);
            }
            return ec;
        }

        private object TransformAttribute(ExportAttribute exportAttribute, Entity entity)
        {
            object attributeValue = null;
            var attributeName = exportAttribute.AttributeName;
            JObject jObject;
            JArray jArray;

            try
            {
                //check the stored attribute type in the file and set the crm entity's attribute values accordingly
                switch (exportAttribute.AttributeType)
                {
                    case "System.Guid":
                        attributeValue = new Guid((string)exportAttribute.AttributeValue);
                        break;

                    case "System.Decimal":
                        attributeValue = Convert.ToDecimal(exportAttribute.AttributeValue);
                        break;

                    case "System.Double":
                        attributeValue = Convert.ToDouble(exportAttribute.AttributeValue);
                        break;

                    case "System.Int32":
                        attributeValue = Convert.ToInt32(exportAttribute.AttributeValue);
                        break;

                    case "Microsoft.Xrm.Sdk.EntityReference":
                        jObject = (JObject)exportAttribute.AttributeValue;
                        EntityReference lookup = new EntityReference((string)jObject["LogicalName"], (Guid)jObject["Id"]);
                        attributeValue = lookup;
                        break;

                    case "Microsoft.Xrm.Sdk.OptionSetValue":
                        jObject = (JObject)exportAttribute.AttributeValue;
                        attributeValue = new OptionSetValue { Value = (int)jObject["Value"] };
                        break;

                    case "Microsoft.Xrm.Sdk.Money":
                        jObject = (JObject)exportAttribute.AttributeValue;
                        attributeValue = new Money { Value = (decimal)jObject["Value"] };
                        break;

                    case "Microsoft.Xrm.Sdk.EntityCollection":
                        jArray = (JArray)exportAttribute.AttributeValue;

                        var childentities = new List<ExportEntity>();
                        foreach (var child in jArray.Children())
                        {
                            ExportEntity childentity = (ExportEntity)JsonConvert.DeserializeObject<ExportEntity>(child.ToString());
                            childentities.Add(childentity);
                        }

                        var ec = new EntityCollection();

                        // TODO.. what is this recursion?
                        foreach (var item in TransformExportedEntityList(childentities))
                        {
                            ec.Entities.Add(item);
                        }

                        attributeValue = ec;
                        break;

                    case "System.Byte[]":
                        attributeValue = Convert.FromBase64String(exportAttribute.AttributeValue.ToString());
                        break;

                    default:
                        attributeValue = exportAttribute.AttributeValue;
                        break;
                }

                return attributeValue;
            }
            catch (Exception)
            {
                Console.WriteLine(string.Format("Error deserializing {3} attribute {0} for entity type {1}, id {2}", attributeName, entity.LogicalName, entity.Id.ToString(), exportAttribute.AttributeType));
                return null;
            }
        }
    }
}