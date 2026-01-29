using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FAM.Application.ExcelImport.PhysicalStockVerification
{
    public class ImportAssetAuditDto
    {   
        public int AuditCycle { get; set; }  
        public IFormFile? File { get; set; }
    }
}