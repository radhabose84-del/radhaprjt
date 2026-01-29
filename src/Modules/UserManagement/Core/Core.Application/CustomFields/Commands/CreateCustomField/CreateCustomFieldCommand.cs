using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.CustomFields.Commands.CreateCustomField
{
    public class CreateCustomFieldCommand : IRequest<int>
    {
        public string? LabelName { get; set; }
        public int Length { get; set; }
        public int DataTypeId { get; set; }
        public int LabelTypeId { get; set; }
        public byte IsRequired { get; set; }
        public List<CustomFieldMenuDto>? Menu { get; set; }
        public List<CustomFieldUnitDto>? Unit { get; set; }
        public List<CustomFieldOptionalValueDto>? OptionalValues { get; set; }
    }
}