using FinanceManagement.Application.TaxCode.Dto;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetGstrSectionMasterById
{
    public class GetGstrSectionMasterByIdQuery : IRequest<GstrSectionMasterDto?>
    {
        public int Id { get; set; }
    }
}
