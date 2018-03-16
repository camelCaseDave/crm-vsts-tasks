using VstsExtensions.Core;

namespace VstsExtensions
{
    public interface IGetPublishedRules : IVstsTask
    {
        string WorkingDirectory { get; }
        IXrmCommand XrmCommand { get; set; }
    }
}