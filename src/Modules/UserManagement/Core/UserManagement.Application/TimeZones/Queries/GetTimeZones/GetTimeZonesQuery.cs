using Contracts.Common;
using MediatR;

namespace UserManagement.Application.TimeZones.Queries.GetTimeZones
{
    public class GetTimeZonesQuery : IRequest<ApiResponseDTO<List<TimeZonesDto>>>
    {
        
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
        
    
}