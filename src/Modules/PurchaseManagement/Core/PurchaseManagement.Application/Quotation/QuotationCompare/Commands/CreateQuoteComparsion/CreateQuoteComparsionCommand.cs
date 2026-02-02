using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.Quotation.QuotationCompare.Commands.CreateQuoteComparsion
{
    public class CreateQuoteComparsionCommand : IRequest<int>
    {
        public CreateQuoteComparsionDto CreateQuoteComparsion { get; set; } = null!;
    }
    
}