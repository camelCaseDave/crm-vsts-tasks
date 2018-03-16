using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace VstsExtensions.Core
{
    public class WorkflowService : IWorkflowService
    {
        private IOrganizationService Service { get; }

        public WorkflowService(IOrganizationService service)
        {
            Service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public EntityCollection RetrieveOthersWorkflows(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(userId), $"Can't retrieve workflows of other users if {nameof(userId)} is an empty guid.");
            }

            var query = new QueryExpression
            {
                EntityName = "workflow",
                ColumnSet = new ColumnSet(false)
            };

            query.Criteria.AddCondition("ownerid", ConditionOperator.NotEqual, userId);

            var workflows = Service.RetrieveMultiple(query);

            Console.WriteLine($"Retrieved {workflows?.Entities?.Count ?? 0} workflows not assigned to user {userId.ToString()}");

            return workflows ?? new EntityCollection();
        }

        public void AssignWorkflowsToUser(EntityCollection workflows, Guid userId)
        {
            if (workflows == null)
            {
                throw new ArgumentNullException(nameof(workflows));
            }

            if (userId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(userId), $"(Can't assign workflows to user if {nameof(userId)} is an empty guid.");
            }

            for (var i = 0; i < workflows.Entities.Count; i++)
            {
                var assign = new AssignRequest();

                try
                {
                    assign.Assignee = new EntityReference("systemuser", userId);
                    assign.Target = workflows.Entities[i].ToEntityReference();

                    Service.Execute(assign);

                    Console.WriteLine($"Assigned workflow {(i + 1)} with ID: {assign.Target.Id} to user with ID: {userId.ToString()}");
                }
                catch (Exception)
                {
                    Console.WriteLine($"Error assigning workflow {(i + 1)} with ID: {assign.Target.Id} to user. Continuing.");
                }
            }
        }
    }
}