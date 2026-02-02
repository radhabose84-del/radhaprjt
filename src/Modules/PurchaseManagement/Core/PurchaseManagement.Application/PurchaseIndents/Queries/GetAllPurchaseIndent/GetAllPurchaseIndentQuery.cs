using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.HttpResponse;
using MediatR;

namespace PurchaseManagement.Application.PurchaseIndents.Queries.GetAllPurchaseIndent
{
    public class GetAllPurchaseIndentQuery : IRequest<ApiResponseDTO<List<PurchaseManagement.Application.PurchaseIndents.Queries.GetAllPurchaseIndent.IndentDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
        public int? StatusId { get; set; }
    }
}