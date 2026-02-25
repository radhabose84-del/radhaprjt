namespace FAM.Domain.Entities.AssetMaster
{
    public class AssetLocation
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public int UnitId { get; set; } 
        public int DepartmentId { get; set; }
        public int LocationId { get; set; }
        public int SubLocationId { get; set; } 
        public int CustodianId { get; set; }
        public int UserID { get; set; }

        public AssetMasterGenerals? Asset { get; set; }
        public Location? Location { get; set; }
        public SubLocation? SubLocation { get; set; }  
      

       
    }
}