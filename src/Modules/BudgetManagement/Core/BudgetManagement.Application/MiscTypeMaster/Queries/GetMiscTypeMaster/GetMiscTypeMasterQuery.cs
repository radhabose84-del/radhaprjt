using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetManagement.Application.Common.HttpResponse;
using MediatR;

namespace BudgetManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster
{
    public class GetMiscTypeMasterQuery  : IRequest<ApiResponseDTO<List<GetMiscTypeMasterDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}