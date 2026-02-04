using InventoryManagement.Domain.Entities.Item.ItemDetail.Templates;

namespace InventoryManagement.Domain.Entities.Item.ItemDetail
{
    public class ItemQuality
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public ItemMaster Item { get; set; } = null!;
        public int? InspectionTemplateId { get; set; }
        public InspectionTemplate? InspectionTemplate { get; set; }  
        public int? CertificateTypeId { get; set; }
        public MiscMaster MiscCertificateType { get; set; } = null!;
        public int? InspLotProcessingTime { get; set; }
        public bool InspectionRequired { get; set; }
        public bool QualityInspectionFree { get; set; }
        public bool IsCertificateRequiredFromSupplier { get; set; }
    }
}