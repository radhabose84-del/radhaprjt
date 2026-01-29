using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.CustomFields.Commands.UpdateCustomField
{
    public class CustomFieldMenuUpdateDto
    {
        public int MenuId { get; set; }
        public int CustomFieldId { get; set; }
    }
}