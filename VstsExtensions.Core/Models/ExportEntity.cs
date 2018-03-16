using System;
using System.Collections.Generic;

namespace VstsExtensions.Core.Models
{
    [Serializable()]
    public class ExportEntity
    {
        public string LogicalName { get; set; }
        public Guid Id { get; set; }
        public List<ExportAttribute> Attributes { get; set; }

        public ExportEntity()
        {
            Attributes = new List<ExportAttribute>(); ;
        }
    }
}