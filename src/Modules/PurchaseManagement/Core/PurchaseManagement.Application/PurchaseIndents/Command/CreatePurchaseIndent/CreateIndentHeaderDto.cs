using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.PurchaseIndents.Command.CreatePurchaseIndent
{
    public class CreateIndentHeaderDto
    {
        public int Id { get; set; }
        public string? IndentNumber { get; set; }
        public DateOnly IndentDate { get; set; }
        public int IndentTypeId { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
        public string? Purpose { get; set; }
        
    }
}