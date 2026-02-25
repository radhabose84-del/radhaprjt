using Microsoft.AspNetCore.Http;

namespace FAM.Application.ExcelImport.PhysicalStockVerification
{
    public class ImportAssetAuditDto
    {   
        public int AuditCycle { get; set; }  
        public IFormFile? File { get; set; }
    }
}