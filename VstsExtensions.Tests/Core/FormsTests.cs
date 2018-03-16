using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;
using System;
using VstsExtensions.Core;

namespace VstsExtensions.Tests.Core
{
    [TestClass]
    public class FormsTests
    {
        private Mock<IOrganizationService> Service { get; set; }
        private FormService Forms { get; set; }
        private string FormIds { get; set; }

        private readonly string FormIdOne = Guid.NewGuid().ToString().Replace("{", "").Replace("}", "").ToString();
        private readonly string FormIdTwo = Guid.NewGuid().ToString().Replace("{", "").Replace("}", "").ToString();

        [TestInitialize]
        public void Setup()
        {
            Service = new Mock<IOrganizationService>();
            Forms = new FormService(Service.Object);
            FormIds = $"[\"{FormIdOne}\",\"{FormIdTwo}\"]";
        }

        [TestCategory("Forms"), TestMethod]
        public void Initialises()
        {
            Assert.IsInstanceOfType(Forms, typeof(FormService));
        }

        [TestCategory("Forms"), TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Throws_If_Service_Is_Null()
        {
            new FormService(null);
        }

        [TestCategory("Forms"), TestMethod]
        public void Deactivation_Doesnt_Throw_If_Empty_FormIds()
        {
            Forms.DeactivateEntityForms(string.Empty);
        }

        [TestCategory("Forms"), TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Deactivate_Throws_If_Empty_FormId()
        {
            Forms.DeactivateEntityForm(Guid.Empty);
        }

        [TestCategory("Forms"), TestMethod]
        public void Deactivation_Creates_Form_Entity()
        {
            var entityId = Guid.NewGuid();
            var entity = new Entity();

            Service.Setup(s => s.Update(It.IsAny<Entity>()))
                .Callback<Entity>(e => entity = e);

            Forms.DeactivateEntityForm(entityId);

            Assert.AreEqual("systemform", entity.LogicalName);
            Assert.AreEqual(entityId, entity.Id);
        }

        [TestCategory("Forms"), TestMethod]
        public void Deactivation_Continues_On_Error()
        {
            Service.SetupSequence(s => s.Update(It.IsAny<Entity>()))
                .Throws(new Exception())
                .Pass();

            Forms.DeactivateEntityForms(FormIds);

            Service.Verify(s => s.Update(It.IsAny<Entity>()), Times.Exactly(2));
        }

        [TestCategory("Forms"), TestMethod]
        public void Calls_Update_Twice()
        {
            Forms.DeactivateEntityForms(FormIds);

            Service.Verify(s => s.Update(It.IsAny<Entity>()), Times.Exactly(2));
        }
    }
}