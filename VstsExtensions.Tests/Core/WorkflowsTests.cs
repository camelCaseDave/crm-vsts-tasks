using Microsoft.Crm.Sdk.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using System;
using System.Collections.Generic;
using VstsExtensions.Core;

namespace VstsExtensions.Tests.Core
{
    [TestClass]
    public class WorkflowsTests
    {
        private Mock<IOrganizationService> Service { get; set; }
        private WorkflowService Workflows { get; set; }
        private EntityCollection WorkflowCollection { get; set; }

        [TestInitialize]
        public void Setup()
        {
            Service = new Mock<IOrganizationService>();
            Workflows = new WorkflowService(Service.Object);
            WorkflowCollection = new EntityCollection(new List<Entity>
            {
                new Entity { Id = Guid.NewGuid(), LogicalName = "workflow" },
                new Entity { Id = Guid.NewGuid(), LogicalName = "workflow" }
            });
        }

        [TestCategory("Workflows"), TestMethod]
        public void Initialises_With_Service()
        {
            Assert.IsInstanceOfType(Workflows, typeof(WorkflowService));
        }

        [TestCategory("Workflows"), TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Throws_Without_Service()
        {
            new WorkflowService(null);
        }

        [TestCategory("Workflows"), TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Retrieve_Others_Workflows_Throws_If_UserId_IsEmpty()
        {
            Workflows.RetrieveOthersWorkflows(Guid.Empty);
        }

        [TestCategory("Workflows"), TestMethod]
        public void Retrieve_Others_Workflows_Queries_With_Correct_Parameters()
        {
            var userId = Guid.NewGuid();
            var query = new QueryExpression();

            Service.Setup(s => s.RetrieveMultiple(It.IsAny<QueryExpression>()))
                .Returns(WorkflowCollection)
                .Callback<QueryExpression>(q => query = q);

            Workflows.RetrieveOthersWorkflows(userId);

            Assert.AreEqual("workflow", query.EntityName);
            Assert.AreEqual(userId, query.Criteria.Conditions[0].Values[0]);
        }

        [TestCategory("Workflows"), TestMethod]
        public void Retrieve_Others_Workflows_Returns_Collection()
        {
            Service.Setup(s => s.RetrieveMultiple(It.IsAny<QueryExpression>()))
                .Returns(WorkflowCollection);

            var workflows = Workflows.RetrieveOthersWorkflows(Guid.NewGuid());

            Assert.AreEqual(WorkflowCollection, workflows);
        }

        [TestCategory("Workflows"), TestMethod]
        public void Retrieve_Others_Workflows_Calls_Service_Once()
        {
            Workflows.RetrieveOthersWorkflows(Guid.NewGuid());

            Service.Verify(s => s.RetrieveMultiple(It.IsAny<QueryExpression>()), Times.Once);
        }

        [TestCategory("Workflows"), TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Assign_Workflows_Throws_If_UserId_IsEmpty()
        {
            Workflows.AssignWorkflowsToUser(WorkflowCollection, Guid.Empty);
        }

        [TestCategory("Workflows"), TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Assign_Workflows_Throws_If_WorkflowCollection_IsNull()
        {
            Workflows.AssignWorkflowsToUser(null, Guid.NewGuid());
        }

        [TestCategory("Workflows"), TestMethod]
        public void Assign_Workflows_Executes_AssignRequest()
        {
            Workflows.AssignWorkflowsToUser(WorkflowCollection, Guid.NewGuid());

            Service.Verify(s => s.Execute(It.IsAny<AssignRequest>()), Times.Exactly(2));
        }

        [TestCategory("Workflows"), TestMethod]
        public void Assigns_To_Given_UserId()
        {
            var userId = Guid.NewGuid();
            var assignRequest = new AssignRequest();

            Service.Setup(s => s.Execute(It.IsAny<AssignRequest>()))
                .Returns(new AssignResponse())
                .Callback<AssignRequest>(a => assignRequest = a);

            Workflows.AssignWorkflowsToUser(WorkflowCollection, userId);

            Assert.AreEqual(userId, assignRequest.Assignee.Id);
        }

        [TestCategory("Workflows"), TestMethod]
        public void Assign_Workflows_Continues_On_Exception()
        {
            Service.SetupSequence(s => s.Execute(It.IsAny<AssignRequest>()))
                .Throws(new Exception())
                .Returns(new AssignResponse());

            Workflows.AssignWorkflowsToUser(WorkflowCollection, Guid.NewGuid());

            Service.Verify(s => s.Execute(It.IsAny<AssignRequest>()), Times.Exactly(2));
        }
    }
}