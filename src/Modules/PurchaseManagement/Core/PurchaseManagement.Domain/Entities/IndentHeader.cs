using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities
{
    public class IndentHeader : BaseEntity
    {
        public string IndentNumber { get; set; }
        public DateOnly IndentDate { get; set; }
        public int IndentTypeId { get; set; }
        public int UnitId { get; set; }
        public string Purpose { get; set; }
        public int DepartmentId { get; set; }
        public int StatusId { get; set; }
        public ICollection<IndentDetail> IndentDetails { get; set; }
        public MiscMaster IndentType { get; set; }
        public MiscMaster Status { get; set; }
    }
}