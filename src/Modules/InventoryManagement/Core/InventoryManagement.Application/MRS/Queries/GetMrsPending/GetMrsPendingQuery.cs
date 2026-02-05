using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagement.Application.Common.HttpResponse;
using MediatR;

namespace InventoryManagement.Application.MRS.Queries.GetMrsPending
{
    public class GetMrsPendingQuery : IRequest<ApiResponseDTO<List<MrsPendingDto>>>
    {
          public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}