using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarehouseManagement.Application.Common.HttpResponse;
using MediatR;

namespace WarehouseManagement.Application.RackMaster.Queries.GetAllRackMaster
{
    public class GetAllRackMasterQuery : IRequest <ApiResponseDTO<List<RackMasterDto>>>
    {
        
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
        
    }
}