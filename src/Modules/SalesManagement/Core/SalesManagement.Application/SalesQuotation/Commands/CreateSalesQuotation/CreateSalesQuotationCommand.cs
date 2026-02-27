using MediatR;
using SalesManagement.Application.SalesQuotation.Dto;

namespace SalesManagement.Application.SalesQuotation.Commands.CreateSalesQuotation
{
    public class CreateSalesQuotationCommand : IRequest<int>
    {
        public CreateSalesQuotationDto? SalesQuotationDetails { get; set; }
    }
}
