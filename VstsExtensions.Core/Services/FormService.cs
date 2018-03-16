using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace VstsExtensions.Core
{
    public class FormService : IFormService
    {
        private IOrganizationService Service { get; }

        public FormService(IOrganizationService service)
        {
            Service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public void DeactivateEntityForms(string formIds)
        {
            var forms = JsonConvert.DeserializeObject<List<Guid>>(formIds) ?? new List<Guid>();

            Console.WriteLine($"Parsed formid JSON into {forms?.Count} individual forms.");

            foreach (var i in forms)
            {
                try
                {
                    DeactivateEntityForm(i);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to deactivate entity form with id: {i}. Continuing. Error: {ex}");
                }
            }
        }

        public void DeactivateEntityForm(Guid formId)
        {
            if (formId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(formId));
            }

            Console.WriteLine($"Deactivating form: {formId.ToString()}");

            var form = new Entity
            {
                LogicalName = "systemform",
                Id = formId,
                Attributes = new AttributeCollection
                {
                    { "formactivationstate", new OptionSetValue(0) }
                }
            };

            Service.Update(form);
        }
    }
}