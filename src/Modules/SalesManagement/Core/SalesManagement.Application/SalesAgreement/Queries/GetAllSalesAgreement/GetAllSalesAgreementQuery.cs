using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesAgreement.Dto;

namespace SalesManagement.Application.SalesAgreement.Queries.GetAllSalesAgreement
{
    public class GetAllSalesAgreementQuery : IRequest<ApiResponseDTO<List<SalesAgreementHeaderDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public string? StatusName { get; set; }
    }
}
