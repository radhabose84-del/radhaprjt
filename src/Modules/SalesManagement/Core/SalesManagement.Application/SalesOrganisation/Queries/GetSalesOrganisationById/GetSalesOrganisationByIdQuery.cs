using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesOrganisation.Dto;

namespace SalesManagement.Application.SalesOrganisation.Queries.GetSalesOrganisationById
{
    public class GetSalesOrganisationByIdQuery : IRequest<ApiResponseDTO<SalesOrganisationDto>>
    {
        public int Id { get; set; }
    }
}
