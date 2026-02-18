using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MainStoreStock.Queries.GetItemStockbyId
{
    public class GetItemStockbyIdQuery : IRequest<MainStoreItemStockDto>
    {
        public string? OldUnitcode { get; set; }
        public string? ItemCode { get; set; }
    }
}