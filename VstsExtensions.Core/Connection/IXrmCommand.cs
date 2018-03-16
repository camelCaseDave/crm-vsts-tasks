using Microsoft.Xrm.Sdk;

namespace VstsExtensions.Core
{
    public interface IXrmCommand
    {
        IOrganizationService GetService(bool refresh = false);
    }
}