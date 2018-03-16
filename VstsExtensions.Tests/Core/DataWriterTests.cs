using Microsoft.Crm.Sdk.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Moq;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using VstsExtensions.Core.Helpers;
using VstsExtensions.Core.Models;
using VstsExtensions.Core.Services;

namespace VstsExtensions.Tests.Core
{
    [TestClass]
    public class DataWriterTests
    {
        private IDataWriterService DataWriterService { get; set; }
        private IDataReaderService DataReaderService { get; set; }
        private IDataTransformService DataTransformService { get; set; }
        private Mock<IOrganizationService> Service { get; set; }
        private IFileSystem FileSystem { get; set; }
        private FileManager FileManager { get; set; }

        private string WorkingDirectory { get; set; }
        private EntityCollection Entities { get; set; }

        [TestInitialize]
        public void Setup()
        {
            WorkingDirectory = @"C:\ExportedData\";
            FileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                // MockFileSystem requires a trailing slash at time of writing. See: https://github.com/tathamoddie/System.IO.Abstractions/issues/208
                { WorkingDirectory, new MockDirectoryData() }
            });

            FileManager = new FileManager(FileSystem);
            Service = new Mock<IOrganizationService>();
            DataTransformService = new DataTransformService();
            DataReaderService = new DataReaderService(Service.Object, FileManager);
            DataWriterService = new DataWriterService(Service.Object, FileManager);

            Entities = new EntityCollection(new List<Entity>
            {
                new Entity("contact", Guid.NewGuid()),
                new Entity("contact", Guid.NewGuid()),
                new Entity{ LogicalName = "contact", Id = Guid.NewGuid(), Attributes = new AttributeCollection {
                    { "fullname", "Joe Bloggs" }
                }}
            });
        }

        [TestMethod]
        public void WritesDataToFile()
        {
            Assert.IsFalse(FileManager.FileSystem.File.Exists(@"C:\ExportedData\ExportedData.json"));
            
            var exportedData = new ExportedData();
            exportedData.RecordSets = new List<ExportEntity>
            {
                new ExportEntity
                {
                    Id = Guid.NewGuid()
                }
            };

            DataWriterService.WriteDataToFile(exportedData, @"C:\ExportedData\");

            Assert.IsTrue(FileManager.FileSystem.File.Exists(@"C:\ExportedData\ExportedData.json"));
        }

        [TestMethod]
        public void ImportsDataToCrm()
        {
            // TODO
        }

        [TestMethod]
        public void RemovesStatusAttributes()
        {
            var entity = new Entity
            {
                Id = Guid.NewGuid(),
                LogicalName = "contact",
                Attributes = new AttributeCollection
                {
                    { "statecode", new OptionSetValue(1) },
                    { "statuscode", new OptionSetValue(2) },
                    { "firstname", "Joe" }
                }
            };

            DataWriterService.RemoveStatusAttributes(entity);

            Assert.IsFalse(entity.Attributes.Contains("statecode"));
            Assert.IsFalse(entity.Attributes.Contains("statuscode"));
            Assert.IsTrue(entity.Attributes.Contains("firstname"));
        }

        [TestMethod]
        public void AssociatesManyToManyWithTwoGuids()
        {
            var entity = new Entity
            {
                Id = Guid.NewGuid(),
                LogicalName = "contact",
                Attributes = new AttributeCollection
                {
                    { "interest", new EntityReference("interest", Guid.NewGuid()) },
                    { "lead", new EntityReference("lead", Guid.NewGuid()) }
                }
            };

            DataWriterService.AssociateManyToMany(entity);

            Service.Verify(s => s.Associate("interest", It.IsAny<Guid>(), It.IsAny<Relationship>(), It.IsAny<EntityReferenceCollection>()), Times.Once());
        }

        [TestMethod]
        public void DoesntAssociateWithoutTwoGuids()
        {
            var entity = new Entity
            {
                Id = Guid.NewGuid(),
                LogicalName = "contact",
                Attributes = new AttributeCollection
                {
                    { "interest", new EntityReference("interest", Guid.NewGuid()) }
                }
            };

            DataWriterService.AssociateManyToMany(entity);

            Service.Verify(s => s.Associate("interest", It.IsAny<Guid>(), It.IsAny<Relationship>(), It.IsAny<EntityReferenceCollection>()), Times.Never());
        }

        [TestMethod]
        public void MapsGuids()
        {
            var interestId = Guid.NewGuid();
            var replacementId = Guid.NewGuid();
            var guidMap = new Dictionary<Guid, Guid>
            {
                { interestId, replacementId }
            };
            var entity = new Entity
            {
                Id = Guid.NewGuid(),
                LogicalName = "contact",
                Attributes = new AttributeCollection
                {
                    { "interest", new EntityReference("interest", interestId) }
                }
            };

            Assert.AreNotEqual(replacementId, entity.GetAttributeValue<EntityReference>("interest").Id);

            DataWriterService.MapEntityGuids(entity, guidMap);

            Assert.AreEqual(replacementId, entity.GetAttributeValue<EntityReference>("interest").Id);
        }

        [TestMethod]
        public void SetStateInvokedWithCorrectArguments()
        {
            var entity = new Entity
            {
                Id = Guid.NewGuid(),
                LogicalName = "contact",
                Attributes = new AttributeCollection
                {
                    { "statecode", new OptionSetValue(1) },
                    { "statuscode", new OptionSetValue(2) }
                }
            };

            var request = new SetStateRequest();

            Service.Setup(s => s.Execute(It.IsAny<SetStateRequest>()))
                .Returns(new SetStateResponse())
                .Callback<SetStateRequest>(s => request = s);

            DataWriterService.SetEntityState(entity);

            Assert.AreEqual(new OptionSetValue(1), request.State);
            Assert.AreEqual(new OptionSetValue(2), request.Status);
            Assert.AreEqual(entity.ToEntityReference(), request.EntityMoniker);
        }

        [TestMethod]
        public void SetsEntityState()
        {
            var entity = new Entity
            {
                Id = Guid.NewGuid(),
                LogicalName = "contact",
                Attributes = new AttributeCollection
                {
                    { "statecode", new OptionSetValue(1) },
                    { "statuscode", new OptionSetValue(2) }
                }
            };

            DataWriterService.SetEntityState(entity);

            Service.Verify(s => s.Execute(It.IsAny<SetStateRequest>()), Times.Once());            
        }
    }
}