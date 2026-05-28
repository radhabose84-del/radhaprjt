using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.Quotation.RfqEntry.Commands.UpsertDraft;

public class UpsertRfqDraftCommand : IRequest<UpsertRfqDraftResult>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanAdd;
    public int? Id { get; set; }                 
    public int? InitiationTypeId { get; set; }
    public int? IndentId { get; set; }

    public List<DraftItemDto>? Items { get; set; }       
    public List<DraftSupplierDto>? Suppliers { get; set; }
}

public record UpsertRfqDraftResult(int Id, string RfqCode);

public class DraftItemDto
{
    public int? Id { get; set; }
    public int ItemId { get; set; }
    public decimal Qty { get; set; }
    public int UomId { get; set; }   
    public int HsnId { get; set; } 
}

public class DraftSupplierDto
{
    public int? Id { get; set; }
    public int? SupplierId { get; set; }
    public string? Name { get; set; } = default!;
    public string? Email { get; set; }      
    public string? Mobile { get; set; }
    public string? Gst { get; set; }
}
