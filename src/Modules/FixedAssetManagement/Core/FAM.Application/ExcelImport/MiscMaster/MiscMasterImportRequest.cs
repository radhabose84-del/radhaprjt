using Microsoft.AspNetCore.Http;

namespace FAM.Application.ExcelImport.MiscMaster
{
    public class MiscMasterImportRequest
    {
          public IFormFile File { get; set; } = default!;
    }
}