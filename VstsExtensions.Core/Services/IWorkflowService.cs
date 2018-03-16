using Microsoft.Xrm.Sdk;
using System;

namespace VstsExtensions.Core
{
    public interface IWorkflowService
    {
        EntityCollection RetrieveOthersWorkflows(Guid userId);

        void AssignWorkflowsToUser(EntityCollection workflows, Guid userId);
    }
}