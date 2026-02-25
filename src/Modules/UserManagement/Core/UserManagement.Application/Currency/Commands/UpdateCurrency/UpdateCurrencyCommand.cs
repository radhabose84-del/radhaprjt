using MediatR;

namespace UserManagement.Application.Currency.Commands.UpdateCurrency
{
    public class UpdateCurrencyCommand  : IRequest<int>
    {
        public int Id { get; set; }
       // public string? Code { get; set; }
        public string? Name { get; set; }
        public byte IsActive { get; set; }
       
    }
}