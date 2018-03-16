using VstsExtensions.Core;

namespace VstsExtensions
{
    public interface IDeactivateForms : IVstsTask
    {
        string FormIds { get; }
        IXrmCommand XrmCommand { get; set; }
    }
}