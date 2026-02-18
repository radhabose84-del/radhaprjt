using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Domain.Entities
{
    public class PreventiveScheduleLog
    {
        public int Id { get; set; }
        public int? PreventiveScheduleId { get; set; }
        public int? PreventiveScheduleDetailId { get; set; }
        public string ActionType { get; set; } = default!;
        public string ChangedFields { get; set; } = default!;
        public string? Remarks { get; set; }
        public string? Source { get; set; }
        public bool IsSuccess  { get; set; }
        public string? ErrorMessage  { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
    }
}