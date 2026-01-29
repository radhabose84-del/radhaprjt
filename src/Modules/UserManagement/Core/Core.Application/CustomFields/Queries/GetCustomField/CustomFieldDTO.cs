using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.CustomFields.Commands.CreateCustomField;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.CustomFields.Queries.GetCustomField
{
    public class CustomFieldDTO
    {
        public int Id { get; set; }
        public string? LabelName { get; set; }
        public int Length { get; set; }
        public byte IsRequired { get; set; }
        public string? LabelType { get; set; }
        public string? DataType { get; set; }
        public Status IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
        
    }
}