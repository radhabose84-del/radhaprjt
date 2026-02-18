using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace InventoryManagement.Application.UOM.Queries.GetUOMs
{
    public class GetUOMQuery  : IRequest<ApiResponseDTO<List<UOMDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
        
    }
}