using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
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
    public class DataReaderTests
    {
        private IDataReaderService DataReaderService { get; set; }
        private IDataWriterService DataWriterService { get; set; }
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
        public void GetsDataFromCrm()
        {
            Service.Setup(s => s.RetrieveMultiple(It.IsAny<FetchExpression>()))
                .Returns(Entities);

            var fetch = @"<fetch distinct='false' mapping='logical' output-format='xml-platform' version='1.0'>
                            <entity name='contact'>
                                <attribute name='fullname' />
                                <attribute name='contactid' />
                               <order descending='false' attribute='fullname' />
                              <filter type='and' >
                             <condition attribute='createdon' operator='today' />
                            </filter>
                           </entity>
                          </fetch>";

            var data = DataReaderService.GetDataFromCrm(fetch);

            Assert.AreEqual(3, data.Count);
        }

        [TestMethod]
        public void ParsesDataFromFile()
        {
            var dataBefore = new ExportedData
            {
                BaseBu = Guid.NewGuid(),
                RecordSets = new List<ExportEntity>
                {
                    new ExportEntity{ Id = Guid.NewGuid() },
                    new ExportEntity{ Id = Guid.NewGuid() },
                    new ExportEntity{ Id = Guid.NewGuid() }
                }
            };

            DataWriterService.WriteDataToFile(dataBefore, WorkingDirectory);

            var dataAfter = DataReaderService.ParseDataFromFile(WorkingDirectory);

            Assert.AreEqual(3, dataAfter.RecordSets.Count);
        }
    }
}