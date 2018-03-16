using VstsExtensions.Core;

namespace VstsExtensions
{
    public interface IPublishRules : IVstsTask
    {
        string WorkingDirectory { get; }
        IXrmCommand XrmCommand { get; set; }
    }
}