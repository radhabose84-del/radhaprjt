using PurchaseManagement.Application.Quotations.QuotationEntry.DTOs;
using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.Quotations.QuotationEntry.Commands.Create;

public record CreateQuotationCommand(
    int RfqId,
    int SupplierId,
    string QuotationNumber,
    DateOnly ValidTill,
    int? FreightModeId,
    decimal Freight,
    int? PaymentTermsId,
    int? IncotermsId,
    decimal? InsuranceCharge,
    string? QuotationImage,
    decimal TaxableSubtotal,
    decimal TaxableGst,
    decimal TaxableTotal,
    decimal GrandTotal,
    IReadOnlyList<QuotationDetailDto> Lines
) : IRequest<int>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanAdd;
}
