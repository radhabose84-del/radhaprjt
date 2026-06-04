using Contracts.Common;
using MediatR;

namespace UserManagement.Application.Station.Queries.GetAllStation
{
    public class GetAllStationQuery : IRequest<ApiResponseDTO<List<GetAllStationDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}
