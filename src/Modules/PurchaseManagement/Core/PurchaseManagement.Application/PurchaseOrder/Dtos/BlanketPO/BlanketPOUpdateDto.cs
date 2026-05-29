namespace PurchaseManagement.Application.PurchaseOrder.Dtos.BlanketPO;

public class BlanketPOUpdateDto : BlanketPOCreateDto
{
    public int Id { get; set; }
    public int StatusId { get; set; }
    public int RevisionNo { get; set; }
    public string? AmendmentReason { get; set; }
}
