using System;
using System.Collections.Generic;

namespace VstsExtensions.Core.Models
{
    [Serializable()]
    public class ExportedData
    {
        public Guid BaseBu { get; set; }
        public Guid BaseTeam { get; set; }
        public Guid BaseCurrency { get; set; }
        public List<ExportEntity> RecordSets { get; set; }

        public ExportedData()
        {
            BaseBu = Guid.Empty;
            BaseTeam = Guid.Empty;
            BaseCurrency = Guid.Empty;
            RecordSets = new List<ExportEntity>();
        }
    }
}