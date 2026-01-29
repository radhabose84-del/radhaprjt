using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FAM.Application.ExcelImport.MiscMaster
{
    public class MiscMasterImportRequest
    {
          public IFormFile File { get; set; }
    }
}