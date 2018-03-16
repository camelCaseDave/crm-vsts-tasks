using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using VstsExtensions.Core.Helpers;

namespace VstsExtensions.Core
{
    public class SlaService : ISlaService
    {
        private IOrganizationService Service { get; }

        public SlaService(IOrganizationService service)
        {
            Service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public void UpdateSLA(Guid id, bool isDefault, bool isActive, EntityReference businessHours = null)
        {
            var sla = new Entity
            {
                LogicalName = "sla",
                Id = id
            };

            sla.Attributes.Add("isdefault", isDefault);
            sla.Attributes.AddRange(isActive ? GetActiveAttributes() : GetInactiveAttributes());

            if (businessHours != null && businessHours.Id != Guid.Empty)
            {
                sla.Attributes.Add("businesshoursid", businessHours);
            }

            Service.Update(sla);
        }

        public EntityReference TryGetBusinessHours(string id)
        {
            var businessHours = new EntityReference("calendar");

            if (!string.IsNullOrEmpty(id))
            {
                var parsedId = GuidParser.TryParseIdOrLog(id, $"Business hours id of {id} is not valid and can't be set on the SLA.");

                if (parsedId.HasValue && parsedId != Guid.Empty)
                {
                    try
                    {
                        var calendar = Service.Retrieve("calendar", parsedId.Value, new ColumnSet(false));

                        if (calendar == null || calendar.Id == Guid.Empty)
                        {
                            Console.WriteLine($"Unable to retrieve business hours for id: {id}. Business hours won't be set on the SLA.");
                        }
                        else
                        {
                            Console.WriteLine("Check successful: business hours exist in CRM.");

                            businessHours = calendar.ToEntityReference();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception thrown while validating business hours with id: {id}. Business hours won't be set on the SLA. Exception: {ex}");
                    }
                }
            }

            return businessHours;
        }

        private AttributeCollection GetActiveAttributes()
        {
            return new AttributeCollection
            {
                { "statecode", new OptionSetValue((int)State.Active) },
                { "statuscode", new OptionSetValue((int)Status.Active) }
            };
        }

        private AttributeCollection GetInactiveAttributes()
        {
            return new AttributeCollection
            {
                { "statecode", new OptionSetValue((int)State.Draft) },
                { "statuscode", new OptionSetValue((int)Status.Draft) }
            };
        }

        public enum State
        {
            Draft = 0,
            Active = 1
        }

        public enum Status
        {
            Draft = 1,
            Active = 2
        }
    }
}