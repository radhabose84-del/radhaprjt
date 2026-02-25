using MediatR;

namespace UserManagement.Application.Currency.Commands.CreateCurrency
{
    public class CreateCurrencyCommand :IRequest<int>
    { 
        public string? Code { get; set; }
        public string? Name { get; set; }

    }
}