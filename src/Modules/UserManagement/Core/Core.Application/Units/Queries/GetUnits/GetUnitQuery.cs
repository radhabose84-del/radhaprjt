using Core.Application.Common;
using Core.Application.Common.HttpResponse;
using MediatR;


namespace Core.Application.Units.Queries.GetUnits
{
    public class GetUnitQuery : IRequest<ApiResponseDTO<List<GetUnitsDTO>>>
    {
        
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }  
  
}