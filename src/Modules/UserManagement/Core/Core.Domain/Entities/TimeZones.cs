using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Common;


namespace Core.Domain.Entities
{
    public class TimeZones : BaseEntity
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Location { get; set; } // New Field
        public string? Offset { get; set; } // New Field (e.g., UTC+05:30)

    }
}