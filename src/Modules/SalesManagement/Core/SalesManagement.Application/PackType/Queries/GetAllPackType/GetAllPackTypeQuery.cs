using Contracts.Common;
using MediatR;
using SalesManagement.Application.PackType.Dto;

namespace SalesManagement.Application.PackType.Queries.GetAllPackType
{
    public class GetAllPackTypeQuery : IRequest<ApiResponseDTO<List<PackTypeDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
