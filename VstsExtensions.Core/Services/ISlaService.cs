using Microsoft.Xrm.Sdk;
using System;

namespace VstsExtensions.Core
{
    public interface ISlaService
    {
        void UpdateSLA(Guid id, bool isDefault, bool isActive, EntityReference businessHours = null);

        EntityReference TryGetBusinessHours(string id);
    }
}