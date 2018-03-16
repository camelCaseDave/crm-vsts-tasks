using VstsExtensions.Core;

namespace VstsExtensions
{
    public interface IUpdateSla : IVstsTask
    {
        bool IsDefault { get; }
        string SlaId { get; }
        bool IsActive { get; }
        string BusinessHoursId { get; }
        IXrmCommand XrmCommand { get; set; }
    }
}