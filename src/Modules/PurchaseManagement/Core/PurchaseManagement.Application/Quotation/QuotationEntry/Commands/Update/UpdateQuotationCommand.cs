using PurchaseManagement.Application.Quotations.QuotationEntry.DTOs;
using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.Quotations.QuotationEntry.Commands.Update;

public record UpdateQuotationCommand(
    int Id,
    int SupplierId,
    int RfqId,
    string QuotationNumber,
    DateOnly ValidTill,    
    int FreightModeId,
    decimal Freight,
    int PaymentTermsId,
    int IncotermsId,
    decimal InsuranceCharge,
     decimal TaxableSubtotal,
    decimal TaxableGst,
    decimal TaxableTotal,
    decimal GrandTotal,
    string QuotationImage,
    IReadOnlyList<QuotationDetailDto> Lines,
    int  IsActive    
) : IRequest<Unit>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanUpdate;
}
