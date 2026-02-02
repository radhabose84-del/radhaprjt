// Core.Application/Quotations/QuotationEntry/Queries/GetQuotationById/GetQuotationByIdQuery.cs
using PurchaseManagement.Application.Quotations.QuotationEntry.DTOs;
using MediatR;

namespace PurchaseManagement.Application.Quotations.QuotationEntry.Queries.GetQuotationById;

public class GetQuotationByIdQuery : IRequest<GetQuotationHeaderDto>
{
    public int Id { get; set; }
}
