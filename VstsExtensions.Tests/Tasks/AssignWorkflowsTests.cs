using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;
using System;
using System.Collections.Generic;
using VstsExtensions.Core;

namespace VstsExtensions.Tests.Tasks
{
    [TestClass]
    public class AssignWorkflowsTests
    {
        private readonly Guid UserId = Guid.NewGuid();
        private Mock<IXrmCommand> XrmCommand { get; set; }
        private Mock<IOrganizationService> Service { get; set; }
        private Mock<IWorkflowService> WorkflowService { get; set; }
        private AssignWorkflowsToUser AssignWorkflows { get; set; }

        [TestInitialize]
        public void Setup()
        {
            XrmCommand = new Mock<IXrmCommand>();
            Service = new Mock<IOrganizationService>();
            WorkflowService = new Mock<IWorkflowService>();
            AssignWorkflows = new AssignWorkflowsToUser(XrmCommand.Object, UserId.ToString(), WorkflowService.Object);
            XrmCommand.Setup(x => x.GetService(false)).Returns(Service.Object);
        }

        [TestMethod]
        public void Initialises_With_UserId()
        {
            Assert.AreEqual(UserId, AssignWorkflows.UserId);
        }

        [TestMethod]
        public void Creates_XrmCommand_When_Given_ConnectionString()
        {
            var assignWorkflows = new AssignWorkflowsToUser("connectionString", UserId.ToString());
            Assert.IsInstanceOfType(assignWorkflows.XrmCommand, typeof(IXrmCommand));
        }

        [TestMethod]
        public void Run_Calls_Get_Service()
        {
            AssignWorkflows.Run();

            XrmCommand.Verify(x => x.GetService(false), Times.Once());
        }

        [TestMethod]
        public void Run_Doesnt_Create_A_Workflow_Service_If_Given_One()
        {
            AssignWorkflows.Run();
            Assert.IsTrue(ReferenceEquals(WorkflowService.Object, AssignWorkflows.WorkflowService));
        }

        [TestMethod]
        public void Run_Creates_A_New_Workflow_Service_If_Not_Given_One()
        {
            AssignWorkflows = new AssignWorkflowsToUser(XrmCommand.Object, UserId.ToString());
            AssignWorkflows.Run();

            Assert.IsFalse(ReferenceEquals(WorkflowService.Object, AssignWorkflows.WorkflowService));
        }

        [TestMethod]
        public void Run_Calls_RetrieveOthersWorkflows()
        {
            AssignWorkflows.Run();
            WorkflowService.Setup(w => w.RetrieveOthersWorkflows(It.Is<Guid>(g => g == UserId)))
               .Returns(new EntityCollection());

            WorkflowService.Verify(w => w.RetrieveOthersWorkflows(UserId), Times.Once());
        }

        [TestMethod]
        public void Run_Assigns_Workflows_To_Given_UserId()
        {
            var workflows = new EntityCollection(new List<Entity>
            {
                new Entity { Id = Guid.NewGuid(), LogicalName = "workflow" },
                new Entity { Id = Guid.NewGuid(), LogicalName = "workflow" }
            });

            WorkflowService.Setup(w => w.RetrieveOthersWorkflows(It.Is<Guid>(g => g == UserId)))
               .Returns(workflows);

            AssignWorkflows.Run();

            WorkflowService.Verify(w => w.AssignWorkflowsToUser(workflows, UserId), Times.Once());
        }
    }
}