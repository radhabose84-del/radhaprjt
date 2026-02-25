namespace FAM.Application.AssetLocation.Queries.GetAssetLocation
{
    public class AssetLocationDto 
    {  
        public int Id { get; set; }
        public int AssetId { get; set; }
        public int UnitId { get; set; } 
        public int DepartmentId { get; set; }
        public int LocationId { get; set; }
        public int SubLocationId { get; set; }         
    }
}