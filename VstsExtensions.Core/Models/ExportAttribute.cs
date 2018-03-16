using System;

namespace VstsExtensions.Core.Models
{
    [Serializable()]
    public class ExportAttribute
    {
        public string AttributeName { get; set; }
        public object AttributeValue { get; set; }
        public string AttributeType { get; set; }
    }
}