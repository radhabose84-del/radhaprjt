#nullable disable

using Contracts.Common;
using MediatR;
using SalesManagement.Application.BusinessUnit.Dto;

namespace SalesManagement.Application.BusinessUnit.Queries.GetBusinessUnitById
{
    public class GetBusinessUnitByIdQuery : IRequest<ApiResponseDTO<BusinessUnitDto>>
    {
        public int Id { get; set; }
    }
}
