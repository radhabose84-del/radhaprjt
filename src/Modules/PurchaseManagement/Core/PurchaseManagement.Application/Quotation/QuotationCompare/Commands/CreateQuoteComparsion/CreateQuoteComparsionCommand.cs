using MediatR;

namespace PurchaseManagement.Application.Quotation.QuotationCompare.Commands.CreateQuoteComparsion
{
    public class CreateQuoteComparsionCommand : IRequest<int>
    {
        public CreateQuoteComparsionDto CreateQuoteComparsion { get; set; } = null!;
    }
    
}