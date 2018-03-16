using System;

namespace VstsExtensions.Core
{
    public interface IFormService
    {
        void DeactivateEntityForms(string formIds);

        void DeactivateEntityForm(Guid formId);
    }
}