#nullable disable
using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesOffice.Dto;

namespace SalesManagement.Application.SalesOffice.Queries.GetSalesOfficeById
{
    public class GetSalesOfficeByIdQuery : IRequest<ApiResponseDTO<SalesOfficeDto>>
    {
        public int Id { get; set; }
    }
}
