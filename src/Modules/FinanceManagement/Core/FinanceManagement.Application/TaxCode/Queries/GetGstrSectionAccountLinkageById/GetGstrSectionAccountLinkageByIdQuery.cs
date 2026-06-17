using FinanceManagement.Application.TaxCode.Dto;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetGstrSectionAccountLinkageById
{
    public class GetGstrSectionAccountLinkageByIdQuery : IRequest<GstrSectionAccountLinkageDto?>
    {
        public int Id { get; set; }
    }
}
