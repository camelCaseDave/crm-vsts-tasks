using VstsExtensions.Core;

namespace VstsExtensions
{
    public sealed class DeactivateForms : IDeactivateForms
    {
        public string FormIds { get; }
        public IXrmCommand XrmCommand { get; set; }

        public DeactivateForms(string connectionString, string formIds)
        {
            FormIds = formIds;
            XrmCommand = new XrmCommand(connectionString);
        }

        public void Run()
        {
            var service = XrmCommand.GetService();

            var core = new FormService(service);
            core.DeactivateEntityForms(FormIds);
        }
    }
}