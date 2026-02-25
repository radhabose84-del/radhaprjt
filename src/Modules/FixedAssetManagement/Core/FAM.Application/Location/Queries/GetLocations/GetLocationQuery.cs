using Contracts.Common;
using MediatR;

namespace FAM.Application.Location.Queries.GetLocations
{
    public class GetLocationQuery : IRequest<ApiResponseDTO<List<LocationDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}