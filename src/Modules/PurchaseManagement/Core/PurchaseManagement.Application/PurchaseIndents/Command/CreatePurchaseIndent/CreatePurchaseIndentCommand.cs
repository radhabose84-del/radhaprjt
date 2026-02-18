using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.PurchaseIndents.Command.CreatePurchaseIndent
{
    public class CreatePurchaseIndentCommand : IRequest<int>
    {
        
        public DateOnly IndentDate { get; set; }
        public int IndentTypeId { get; set; }
        public int UnitId { get; set; }
        public string? Purpose { get; set; }
        public int DepartmentId { get; set; }
        public byte IsDraft { get; set; }
        public ICollection<IndentDetailDto> IndentDetails { get; set; } = default!;
    }
}