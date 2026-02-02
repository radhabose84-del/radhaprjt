using AutoMapper;
using PurchaseManagement.Application.Quotation.QuotationCompare.Commands.CreateQuoteComparision;
using PurchaseManagement.Application.Quotation.QuotationCompare.Commands.CreateQuoteComparsion;

public class QuotationCompareWorkflowProfile : Profile
{
    public QuotationCompareWorkflowProfile()
    {
        CreateMap<QuoteComparisonWorkFlowDto, CreateQuoteComparisonReverseDto>()
            .ConvertUsing(src => new CreateQuoteComparisonReverseDto
            {
                Header = src,
                Lines  = Array.Empty<QuoteComparisonWorkFlowDto>() // fill later if needed
            });
    }
}
