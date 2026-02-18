using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MRS.Queries.GetPendingQty
{
    public class GetPendingQtyQuery : IRequest<GetPendingQtyDto>
    {
        public string? OldUnitcode { get; set; } 
        public string? ItemCode { get; set; }
    }
}