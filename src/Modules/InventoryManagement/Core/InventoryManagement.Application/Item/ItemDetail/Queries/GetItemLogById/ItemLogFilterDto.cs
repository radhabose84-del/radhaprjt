namespace InventoryManagement.Application.Item.ItemDetail.Queries.GetItemLogById;

public sealed class ItemLogFilter
{
    public int? Page { get; set; }      
    public int? Size { get; set; }      
    public string? Search { get; set; }     
    public int? EntityId { get; set; }    
    public DateTime? From { get; set; } 
    public DateTime? To { get; set; }   
    public int? UserId { get; set; }    
}

