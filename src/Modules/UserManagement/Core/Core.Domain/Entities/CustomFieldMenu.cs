using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Domain.Entities
{
    public class CustomFieldMenu
    {
        public int Id { get; set; }
        public int CustomFieldId { get; set; }
        public CustomField? CustomField { get; set; }
        public int MenuId { get; set; }
        public Menu? Menu { get; set; }
    }
}