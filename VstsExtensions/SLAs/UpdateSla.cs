using VstsExtensions.Core;
using VstsExtensions.Core.Helpers;

namespace VstsExtensions
{
    public sealed class UpdateSla : IUpdateSla
    {
        public bool IsDefault { get; }
        public string SlaId { get; }
        public bool IsActive { get; }
        public string BusinessHoursId { get; }
        public IXrmCommand XrmCommand { get; set; }

        public UpdateSla(string connectionString, string slaId, bool isActive, bool isDefault, string businessHoursId)
        {
            SlaId = slaId;
            IsActive = isActive;
            IsDefault = isDefault;
            BusinessHoursId = businessHoursId;
            XrmCommand = new XrmCommand(connectionString);
        }

        public void Run()
        {
            var service = XrmCommand.GetService();

            var core = new SlaService(service);
            var parsedId = GuidParser.TryParseIdOrThrow(SlaId);
            var parsedBusinessId = core.TryGetBusinessHours(BusinessHoursId);

            core.UpdateSLA(parsedId, IsDefault, IsActive, parsedBusinessId);
        }
    }
}