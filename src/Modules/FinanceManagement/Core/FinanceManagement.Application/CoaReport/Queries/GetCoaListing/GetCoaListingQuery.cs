using Contracts.Common;
using FinanceManagement.Application.CoaReport.Dto;
using MediatR;

namespace FinanceManagement.Application.CoaReport.Queries.GetCoaListing
{
    // US-GL02-15 (AC1) — COA listing grid (hierarchy + attributes + posting count + FS-mapping).
    public class GetCoaListingQuery : IRequest<ApiResponseDTO<List<CoaListingItemDto>>>
    {
        public int? AccountTypeId { get; set; }
        public int? AccountGroupId { get; set; }
        public bool ActiveOnly { get; set; }
        public string? SearchTerm { get; set; }
    }
}
