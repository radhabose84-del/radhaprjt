using Core.Application.Common;
using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.Entity.Queries.GetEntity
{
    public class GetEntityQuery : IRequest<ApiResponseDTO<List<GetEntityDTO>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }   
    }
}