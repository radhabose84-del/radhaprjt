using MediatR;
using SalesManagement.Application.SalesOrganisation.Dto;

namespace SalesManagement.Application.SalesOrganisation.Queries.GetSalesOrganisationById
{
    public class GetSalesOrganisationByIdQuery : IRequest<SalesOrganisationDto?>
    {
        public int Id { get; set; }
    }
}
