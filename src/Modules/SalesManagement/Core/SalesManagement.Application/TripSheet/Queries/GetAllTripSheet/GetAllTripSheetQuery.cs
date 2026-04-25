using Contracts.Common;
using MediatR;
using SalesManagement.Application.TripSheet.Dto;

namespace SalesManagement.Application.TripSheet.Queries.GetAllTripSheet
{
    public class GetAllTripSheetQuery : IRequest<ApiResponseDTO<List<TripSheetHeaderDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
