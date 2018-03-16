using System;
using VstsExtensions.Core;

namespace VstsExtensions
{
    public interface IAssignWorkflowsToUser : IVstsTask
    {
        Guid UserId { get; }
        IXrmCommand XrmCommand { get; }
        IWorkflowService WorkflowService { get; }
    }
}