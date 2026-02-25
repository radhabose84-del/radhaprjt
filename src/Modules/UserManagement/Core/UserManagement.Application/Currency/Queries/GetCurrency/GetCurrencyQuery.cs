using Contracts.Common;
using MediatR;

namespace UserManagement.Application.Currency.Queries.GetCurrency
{
    public class GetCurrencyQuery : IRequest<ApiResponseDTO<List<CurrencyDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }    
    
}