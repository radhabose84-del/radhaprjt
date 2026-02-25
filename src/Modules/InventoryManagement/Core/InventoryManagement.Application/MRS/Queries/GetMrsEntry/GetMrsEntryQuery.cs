using Contracts.Common;
using MediatR;

namespace InventoryManagement.Application.MRS.Queries.GetMrsEntry
{
    public class GetMrsEntryQuery : IRequest<ApiResponseDTO<List<GetMrsEntryDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
    }
}