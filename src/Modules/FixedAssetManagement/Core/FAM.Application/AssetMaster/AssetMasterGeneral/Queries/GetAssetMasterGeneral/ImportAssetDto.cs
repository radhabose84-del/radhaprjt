using Microsoft.AspNetCore.Http;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral
{
    public class ImportAssetDto
    {        
        public int CompanyId { get; set; }  
        public int UnitId { get; set; }          
        public IFormFile? File { get; set; }
    }
}