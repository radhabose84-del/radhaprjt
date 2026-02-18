using UserManagement.Application.Common;
using Contracts.Common;
using MediatR;

namespace UserManagement.Application.Entity.Queries.GetEntity
{
    public class GetEntityQuery : IRequest<ApiResponseDTO<List<GetEntityDTO>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }   
    }
}