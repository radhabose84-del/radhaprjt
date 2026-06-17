using FinanceManagement.Application.TaxCode.Dto;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetTaxAccountLinkageById
{
    public class GetTaxAccountLinkageByIdQuery : IRequest<TaxAccountLinkageDto?>
    {
        public int Id { get; set; }
    }
}
