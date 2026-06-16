using FinanceManagement.Application.TaxCode.Dto;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetGstrSectionMappingById
{
    public class GetGstrSectionMappingByIdQuery : IRequest<GstrSectionMappingDto?>
    {
        public int Id { get; set; }
    }
}
