using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Domain.Entities
{
    public class CustomFieldOptionalValue
    {
        public int Id { get; set; }
        public int CustomFieldId { get; set; }
        public CustomField? CustomField { get; set; }
        public string? OptionFieldValue { get; set; }
    }
}