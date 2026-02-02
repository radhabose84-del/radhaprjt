using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.PurchaseIndents.Queries.ApprovedIndentDetailsForPO
{
    public class IndentForPODto
    {
        public int Id { get; set; }
        public string IndentNumber { get; set; }
        public DateOnly IndentDate { get; set; }
        public int IndentTypeId { get; set; }
        public string IndentTypeName { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public string Purpose { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int? RfqId { get; set; }
        public int? QuotationId { get; set; }
        public int? HasPriceMaster { get; set; }  
        public string? PendingReason { get; set; }
        
        public ICollection<IndentDetailsForPODto> IndentDetails { get; set; }
        public ICollection<IndentDutyForPODto> IndentDutyDetails { get; set; }
    }
}