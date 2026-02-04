using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Application.CustomFields.Commands.UpdateCustomField
{
    public class CustomFieldUnitUpdateDto
    {
        public int UnitId { get; set; }
        public int CustomFieldId { get; set; }
    }
}