using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecification
{
    public class AssetSpecificationDTO
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public string? AssetName { get; set; }
        public string? AssetCode { get; set; }
      /*   public int ManufactureId { get; set; }
        public string? ManufactureName { get; set; }
        public DateTimeOffset? ManufactureDate { get; set; }  */
        public int SpecificationId { get; set; }        
        public string? SpecificationName { get; set; }
        public string? SpecificationValue { get; set; }/* 
        public string? SerialNumber { get; set; }
        public string? ModelNumber { get; set; } */
        public Status IsActive { get; set; }
        public IsDelete IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset?  CreatedAt { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset?  ModifiedAt { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; } 
    }
}