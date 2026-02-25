using MediatR;

namespace UserManagement.Application.Currency.Commands.DeleteCurrency
{
    public class DeleteCurrencyCommand : IRequest<int>
    {
         public int Id { get; set; }
    }
}