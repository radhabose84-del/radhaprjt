using Contracts.Common;
using MediatR;


namespace UserManagement.Application.Units.Queries.GetUnits
{
    public class GetUnitQuery : IRequest<ApiResponseDTO<List<GetUnitsDTO>>>
    {
        
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }  
  
}