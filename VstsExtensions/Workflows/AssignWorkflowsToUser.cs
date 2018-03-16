using System;
using VstsExtensions.Core;
using VstsExtensions.Core.Helpers;

namespace VstsExtensions
{
    public sealed class AssignWorkflowsToUser : IAssignWorkflowsToUser
    {
        public Guid UserId { get; }
        public IXrmCommand XrmCommand { get; }
        public IWorkflowService WorkflowService { get; set; }

        public AssignWorkflowsToUser(string connectionString, string userId)
        {
            XrmCommand = new XrmCommand(connectionString);
            UserId = GuidParser.TryParseIdOrThrow(userId);
        }

        public AssignWorkflowsToUser(IXrmCommand xrmCommand, string userId, IWorkflowService service = null)
        {
            XrmCommand = xrmCommand;
            UserId = GuidParser.TryParseIdOrThrow(userId);
            WorkflowService = service;
        }

        public void Run()
        {
            var service = XrmCommand.GetService();

            var core = WorkflowService ?? new WorkflowService(service);
            var workflows = core.RetrieveOthersWorkflows(UserId);

            core.AssignWorkflowsToUser(workflows, UserId);
        }
    }
}