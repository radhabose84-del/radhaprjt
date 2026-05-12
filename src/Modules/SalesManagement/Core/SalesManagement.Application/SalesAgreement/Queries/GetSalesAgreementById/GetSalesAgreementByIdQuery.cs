using MediatR;
using SalesManagement.Application.SalesAgreement.Dto;

namespace SalesManagement.Application.SalesAgreement.Queries.GetSalesAgreementById
{
    public class GetSalesAgreementByIdQuery : IRequest<SalesAgreementHeaderDto?>
    {
        public int Id { get; set; }
    }
}
