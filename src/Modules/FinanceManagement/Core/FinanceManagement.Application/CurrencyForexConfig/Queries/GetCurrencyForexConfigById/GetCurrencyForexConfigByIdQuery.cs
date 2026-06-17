using FinanceManagement.Application.CurrencyForexConfig.Dto;
using MediatR;

namespace FinanceManagement.Application.CurrencyForexConfig.Queries.GetCurrencyForexConfigById
{
    public class GetCurrencyForexConfigByIdQuery : IRequest<CurrencyForexConfigDto?>
    {
        public int Id { get; set; }
    }
}
