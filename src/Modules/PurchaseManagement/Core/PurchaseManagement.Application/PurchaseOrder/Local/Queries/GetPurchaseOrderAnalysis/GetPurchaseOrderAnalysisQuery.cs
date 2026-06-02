using MediatR;
using PurchaseManagement.Application.Common;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;

namespace PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPurchaseOrderAnalysis;

/// <summary>
/// PO Analysis list query — supports status tabs (StatusId), Amendment filter (RevisionNo &gt; 0),
/// PO date range (FromDate/ToDate), plus search / PO method / budget group filtering.
/// </summary>
public record GetPurchaseOrderAnalysisQuery(
    int PageNumber,
    int PageSize,
    string? SearchTerm,
    int? PoMethodId,
    int? StatusId,
    DateTimeOffset? FromDate,
    DateTimeOffset? ToDate,
    bool? IsAmendment
) : IRequest<PagedResult<PurchaseOrderAnalysisListItemDto>>;
