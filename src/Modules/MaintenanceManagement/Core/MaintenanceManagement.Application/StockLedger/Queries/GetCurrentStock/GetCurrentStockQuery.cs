using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.StockLedger.Queries.GetCurrentStock
{
    public class GetCurrentStockQuery : IRequest<ApiResponseDTO<CurrentStockDto>>
    {
        public string? OldUnitId { get; set; }
        public string? ItemCode { get; set; }
        public int DepartmentId { get; set; }

    }
}