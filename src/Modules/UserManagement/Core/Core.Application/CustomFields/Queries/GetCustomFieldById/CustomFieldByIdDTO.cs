using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.CustomFields.Commands.CreateCustomField;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.CustomFields.Queries.GetCustomFieldById
{
    public class CustomFieldByIdDTO
    {
         public int Id { get; set; }
        public string? LabelName { get; set; }
        public int Length { get; set; }
        public bool IsRequired { get; set; }
        public int LabelTypeId { get; set; }
        public int DataTypeId { get; set; }
        public bool IsActive { get; set; }
        public List<CustomFieldMenuDto>? Menu { get; set; }
        public List<CustomFieldUnitDto>? Unit { get; set; }
        public List<CustomFieldOptionalValueDto>? OptionalValues { get; set; }
    }
}