using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Domain.Entities.PurchaseOrder;


namespace PurchaseManagement.Domain.PurchaseOrder
{
    public class PurchaseDocument 
    {
        public int Id { get; set; }
        public int PoId { get; set; }
        public PurchaseOrderHeader PODocumentId { get; set; } = null!;
        public int DocumentId { get; set; }
        public MiscMaster? MiscMaster { get; set; } 
        public string? FileName { get; set; }
        public DateTimeOffset UploadedDate { get; set; }       
         
    }
}
