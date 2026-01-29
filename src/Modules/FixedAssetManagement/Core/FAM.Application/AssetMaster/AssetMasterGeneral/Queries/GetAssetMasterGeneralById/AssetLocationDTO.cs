
namespace FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneralById
{
    public class AssetLocationDTO
    {
        public string? UnitName { get; set; }
        public string? DeptName { get; set; }
        public string? LocationName { get; set; }
        public string? SubLocationName { get; set; }        
        public int CustodianId { get; set; } 
        public string? CustodianName { get; set; }
        public int UserId { get; set; }      
        
        public string? UserName { get; set; }
        public string? OldUnitId { get; set; }  
        public int DepartmentId { get; set; }
        public int LocationId { get; set; }
        public int SubLocationId { get; set; } 

    }
}