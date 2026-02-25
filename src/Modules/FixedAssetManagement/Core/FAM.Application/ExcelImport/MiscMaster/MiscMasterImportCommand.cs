using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FAM.Application.ExcelImport.MiscMaster
{
    public class MiscMasterImportCommand : IRequest<ApiResponseDTO<bool>>
    {

      
        public IFormFile File { get; set; }
          public MiscMasterImportCommand(IFormFile file)
        {
            File = file;
        }
           
       

    }
}