using FinanceManagement.Application.TaxCode.Dto;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetTaxCodeMasterById
{
    public class GetTaxCodeMasterByIdQuery : IRequest<TaxCodeMasterDto?>
    {
        public int Id { get; set; }
    }
}
