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
        public string ActionType { get; set; }
        public string ActionRemarks { get; set; }
        public string PreviousData { get; set; }
        public string NewData { get; set; }
        public int StatusId { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string CreatedByName { get; set; }
        public string CreatedIP { get; set; }
    }
}