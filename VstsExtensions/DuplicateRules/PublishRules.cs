using VstsExtensions.Core;

namespace VstsExtensions
{
    public sealed class PublishRules : IPublishRules
    {
        public string WorkingDirectory { get; }
        public IXrmCommand XrmCommand { get; set; }

        public PublishRules(string connectionString, string workingDirectory)
        {
            WorkingDirectory = workingDirectory;
            XrmCommand = new XrmCommand(connectionString);
        }

        public void Run()
        {
            var service = XrmCommand.GetService();

            var core = new DuplicateRuleService(service);
            var rules = core.GetRulesFromConfig(WorkingDirectory);

            core.PublishRules(rules);
        }
    }
}