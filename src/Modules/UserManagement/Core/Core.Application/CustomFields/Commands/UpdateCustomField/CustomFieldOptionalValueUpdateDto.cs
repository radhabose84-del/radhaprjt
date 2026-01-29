using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.CustomFields.Commands.UpdateCustomField
{
    public class CustomFieldOptionalValueUpdateDto
    {
        public string? OptionFieldValue { get; set; }
        public int CustomFieldId { get; set; }
    }
}