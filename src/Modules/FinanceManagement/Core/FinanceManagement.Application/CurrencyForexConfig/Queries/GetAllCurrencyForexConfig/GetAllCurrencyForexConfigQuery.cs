using Contracts.Common;
using FinanceManagement.Application.CurrencyForexConfig.Dto;
using MediatR;

namespace FinanceManagement.Application.CurrencyForexConfig.Queries.GetAllCurrencyForexConfig
{
    public class GetAllCurrencyForexConfigQuery : IRequest<ApiResponseDTO<List<CurrencyForexConfigDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
