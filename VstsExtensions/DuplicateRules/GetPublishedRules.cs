using VstsExtensions.Core;

namespace VstsExtensions
{
    public class GetPublishedRules : IGetPublishedRules
    {
        public string WorkingDirectory { get; }
        public IXrmCommand XrmCommand { get; set; }

        public GetPublishedRules(string connectionString, string workingDirectory)
        {
            WorkingDirectory = workingDirectory;
            XrmCommand = new XrmCommand(connectionString);
        }

        public void Run()
        {
            var service = XrmCommand.GetService();

            var core = new DuplicateRuleService(service);
            var rules = core.RetrievePublishedRules();

            core.StorePublishedRules(rules, WorkingDirectory);
        }
    }
}