namespace FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneralById
{
    public class AssetSpecDTO
    {
        public int Id { get; set; }
        public string? SpecificationName { get; set; }
        public string? SpecificationValue { get; set; }
        public int SpecificationId { get; set; }
        public bool IsDefault { get; set; }   
    }
}