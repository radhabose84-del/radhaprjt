
namespace PurchaseManagement.Application.PriceMaster.Queries.GetPriceMasterPending;
public sealed class PriceMasterPendingGroupDto
{
    public int Id { get; set; } 
    public int ItemId { get; set; }
    public int VendorId { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public int UomId { get; set; }    
    public string CreatedByName { get; set; }   = string.Empty;
    public DateTimeOffset CreatedDate { get; set; }

    public string SupplierName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;        
    public string UOM { get; set; } = string.Empty;    
    

    public int ApproverId { get; set; }
    public string? ApproverName { get; set; }
    public int ApprovalRequestHeaderId { get; set; }  
    public byte IsEdit { get; set; }
    public List<PriceMasterPendingDto> Lines { get; set; } = new();
}
