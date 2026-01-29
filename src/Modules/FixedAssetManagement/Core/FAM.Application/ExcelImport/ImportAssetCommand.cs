using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using FAM.Application.Common.HttpResponse;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FAM.Application.ExcelImport
{
   public class ImportAssetCommand : IRequest<ApiResponseDTO<bool>>
    {
        public ImportAssetDto ImportDto { get; set; }

        public ImportAssetCommand(ImportAssetDto importDto)
        {
            ImportDto = importDto;
        }
    }
}