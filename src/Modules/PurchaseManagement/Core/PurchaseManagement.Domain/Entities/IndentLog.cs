using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Domain.Entities
{
    public class IndentLog
    {
        public int Id { get; set; }
        public int IndentHeaderId { get; set; }
        public string ActionType { get; set; } = default!;
        public string ActionRemarks { get; set; } = default!;
        public string PreviousData { get; set; } = default!;
        public string NewData { get; set; } = default!;
        public int StatusId { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string CreatedByName { get; set; } = default!;
        public string CreatedIP { get; set; } = default!;
    }
}