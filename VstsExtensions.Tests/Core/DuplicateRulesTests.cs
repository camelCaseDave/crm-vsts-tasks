using Microsoft.Crm.Sdk.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using VstsExtensions.Core;
using VstsExtensions.Core.Helpers;

namespace VstsExtensions.Tests.Core
{
    [TestClass]
    public class DuplicateRulesTests
    {
        private DuplicateRuleService DuplicateRuleService { get; set; }
        private FileManager FileManager { get; set; }
        private Mock<IOrganizationService> Service { get; set; }
        private IFileSystem FileSystem { get; set; }
        private List<Guid> Guids { get; set; }

        [TestInitialize]
        public void Setup()
        {
            FileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                // MockFileSystem requires a trailing slash at time of writing. See: https://github.com/tathamoddie/System.IO.Abstractions/issues/208
                { @"C:\MyGuids\", new MockDirectoryData() }
            });

            FileManager = new FileManager(FileSystem);
            Service = new Mock<IOrganizationService>();
            DuplicateRuleService = new DuplicateRuleService(Service.Object, FileManager);
            Guids = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid() };
        }

        [TestMethod]
        public void Initialises()
        {
            Assert.IsInstanceOfType(DuplicateRuleService, typeof(DuplicateRuleService));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Throws_If_Service_IsNull()
        {
            new DuplicateRuleService(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Stores_Rules_Throws_If_Directory_IsEmpty()
        {
            DuplicateRuleService.StorePublishedRules(Guids, "");
        }

        [TestMethod]
        public void Stores_Rules_As_Json_In_Directory()
        {
            Assert.IsFalse(FileManager.FileSystem.File.Exists(@"C:\MyGuids\PublishedDuplicateDetectionRules.json"));

            DuplicateRuleService.StorePublishedRules(Guids, @"C:\MyGuids");

            Assert.IsTrue(FileManager.FileSystem.File.Exists(@"C:\MyGuids\PublishedDuplicateDetectionRules.json"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Get_Rules_Throws_If_Directory_IsEmpty()
        {
            DuplicateRuleService.GetRulesFromConfig("");
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void Get_Rules_Throws_If_Directory_Not_Found()
        {
            DuplicateRuleService.GetRulesFromConfig(@"C:\SomeoneElsesGuids");
        }

        [TestMethod]
        public void Gets_Rules_Formatted_From_Directory()
        {
            var directory = @"C:\MyGuids";

            DuplicateRuleService.StorePublishedRules(Guids, directory);

            Assert.IsTrue(FileManager.FileSystem.File.Exists(@"C:\MyGuids\PublishedDuplicateDetectionRules.json"));

            var rules = DuplicateRuleService.GetRulesFromConfig(directory);

            CollectionAssert.AreEqual(Guids, rules);
        }

        [TestMethod]
        public void Publish_Continues_On_Error()
        {
            Service.SetupSequence(s => s.Execute(It.IsAny<PublishDuplicateRuleRequest>()))
                .Throws(new Exception())
                .Returns(new OrganizationResponse());

            DuplicateRuleService.PublishRules(Guids);

            Service.Verify(s => s.Execute(It.IsAny<PublishDuplicateRuleRequest>()), Times.Exactly(2));
        }

        [TestMethod]
        public void Retrieve_Rules_Returns_Some_Rules()
        {
            Service.Setup(s => s.RetrieveMultiple(It.IsAny<QueryExpression>()))
                .Returns(new EntityCollection(new List<Entity>()
                {
                    new Entity{ LogicalName = "duplicaterule", Id = Guids[0] },
                    new Entity{ LogicalName = "duplicaterule", Id = Guids[1] }
                }));

            var rules = DuplicateRuleService.RetrievePublishedRules();

            Assert.IsTrue(rules.TrueForAll(r => r != Guid.Empty));
        }

        [TestMethod]
        public void Retrieve_Rules_Queries_For_Published_DuplicateRules()
        {
            var query = new QueryExpression();

            Service.Setup(s => s.RetrieveMultiple(It.IsAny<QueryExpression>()))
                .Returns(new EntityCollection())
                .Callback<QueryExpression>(q => query = q);

            DuplicateRuleService.RetrievePublishedRules();

            Assert.AreEqual("duplicaterule", query.EntityName);
            Assert.AreEqual(2, query.Criteria.Conditions[0].Values[0]);
        }
    }
}