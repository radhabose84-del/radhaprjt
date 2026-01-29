using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Common;

namespace Core.Domain.Entities
{
    public class CustomField : BaseEntity
    {
        public int Id { get; set; }
        public string? LabelName { get; set; }
        public int DataTypeId { get; set; }
        public MiscMaster? DataType { get; set; }
        public int? Length { get; set; }
        public int LabelTypeId { get; set; }
        public MiscMaster? LabelType { get; set; }
        public byte IsRequired { get; set; }
        public IList<CustomFieldMenu> CustomFieldMenu { get; set; } = new List<CustomFieldMenu>();
        public IList<CustomFieldUnit> CustomFieldUnits { get; set; } = new List<CustomFieldUnit>();
        public IList<CustomFieldOptionalValue> CustomFieldOptionalValues { get; set; } = new List<CustomFieldOptionalValue>();
        // public IList<CustomFieldMenu> CustomFieldMenu { get; set; }
        // public IList<CustomFieldUnit> CustomFieldUnits { get; set; }
        // public IList<CustomFieldOptionalValue> CustomFieldOptionalValues { get; set; }


    }
}