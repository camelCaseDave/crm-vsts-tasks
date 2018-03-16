using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using System;
using VstsExtensions.Core;

namespace VstsExtensions.Tests.Core
{
    [TestClass]
    public class SlaTests
    {
        private Mock<IOrganizationService> Service { get; set; }
        private SlaService Sla { get; set; }
        private Entity Entity { get; set; }

        [TestInitialize]
        public void Setup()
        {
            Service = new Mock<IOrganizationService>();
            Sla = new SlaService(Service.Object);
            Entity = new Entity();
            Service.Setup(s => s.Update(It.IsAny<Entity>()))
                .Callback<Entity>(e => Entity = e);
        }

        [TestMethod]
        public void Initialises()
        {
            Assert.IsInstanceOfType(Sla, typeof(SlaService));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Throws_If_Service_IsNull()
        {
            new SlaService(null);
        }

        [TestMethod]
        public void Updates_Sla_With_Id()
        {
            var id = Guid.NewGuid();

            Sla.UpdateSLA(id, true, true);

            Assert.AreEqual(id, Entity.Id);
            Assert.AreEqual("sla", Entity.LogicalName);
        }

        [TestMethod]
        public void Update_Sla_Calls_Service()
        {
            var entityId = Guid.NewGuid();

            Sla.UpdateSLA(entityId, false, false);

            Service.Verify(s => s.Update(It.Is<Entity>(e => e.Id == entityId)));
        }

        [TestMethod]
        public void Update_Sla_Applies_IsDefault_For_True()
        {
            Sla.UpdateSLA(Guid.NewGuid(), isDefault: true, isActive: true);

            Assert.IsTrue(Entity.GetAttributeValue<bool>("isdefault"));
        }

        [TestMethod]
        public void Update_Sla_Applies_IsDefault_For_False()
        {
            Sla.UpdateSLA(Guid.NewGuid(), isDefault: false, isActive: true);

            Assert.IsFalse(Entity.GetAttributeValue<bool>("isdefault"));
        }

        [TestMethod]
        public void Update_Sla_Applies_IsActive_Statuses()
        {
            Sla.UpdateSLA(Guid.NewGuid(), false, isActive: true);

            Assert.AreEqual((int)SlaService.State.Active, Entity.GetAttributeValue<OptionSetValue>("statecode").Value);
            Assert.AreEqual((int)SlaService.Status.Active, Entity.GetAttributeValue<OptionSetValue>("statuscode").Value);
        }

        [TestMethod]
        public void Update_Sla_Applies_IsInactive_Statuses()
        {
            Sla.UpdateSLA(Guid.NewGuid(), false, isActive: false);

            Assert.AreEqual((int)SlaService.State.Draft, Entity.GetAttributeValue<OptionSetValue>("statecode").Value);
            Assert.AreEqual((int)SlaService.Status.Draft, Entity.GetAttributeValue<OptionSetValue>("statuscode").Value);
        }

        [TestMethod]
        public void Doesnt_Add_Business_Hours_If_Null()
        {
            Sla.UpdateSLA(Guid.NewGuid(), true, true);

            Assert.IsNull(Entity.GetAttributeValue<object>("businesshoursid"));
        }

        [TestMethod]
        public void Adds_Business_Hours_If_Not_Null()
        {
            var businessHours = new EntityReference { LogicalName = "calendar", Id = Guid.NewGuid() };

            Sla.UpdateSLA(Guid.NewGuid(), true, true, businessHours);

            Assert.AreEqual(businessHours, Entity.GetAttributeValue<object>("businesshoursid"));
        }

        [TestMethod]
        public void Returns_No_Business_Hours_Given_No_Id()
        {
            var businessHours = Sla.TryGetBusinessHours("");

            Assert.AreEqual(Guid.Empty, businessHours.Id);
        }

        [TestMethod]
        public void Returns_No_Business_Hours_Given_Invalid_Id()
        {
            var businessHours = Sla.TryGetBusinessHours("abc");

            Assert.AreEqual(Guid.Empty, businessHours.Id);
        }

        [TestMethod]
        public void Gets_Calendar_Given_Valid_Id()
        {
            var id = Guid.NewGuid();

            Service.Setup(s => s.Retrieve(
                It.Is<string>(c => c == "calendar"),
                It.Is<Guid>(g => g == id),
                It.IsAny<ColumnSet>()))
                .Returns(new Entity("calendar", id));

            var businessHours = Sla.TryGetBusinessHours(id.ToString());

            Assert.AreEqual(id, businessHours.Id);
            Assert.AreEqual("calendar", businessHours.LogicalName);
        }
    }
}