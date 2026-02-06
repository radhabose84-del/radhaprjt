using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartyManagement.Domain.Entities
{
    public class PartyActivityLog
    {
        public int Id { get; set; }
        public int PartyId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string? ColumnName { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string ActionType { get; set; } = string.Empty; // Insert, Update, Delete
        public int ChangedBy { get; set; }
        public string ChangedByName { get; set; } = string.Empty;
        public string ChangedIp { get; set; } = string.Empty;
        public DateTimeOffset ChangedOn { get; set; } = DateTimeOffset.Now;

    }
}