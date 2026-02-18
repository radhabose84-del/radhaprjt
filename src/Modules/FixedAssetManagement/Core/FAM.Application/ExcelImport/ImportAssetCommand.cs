using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using Contracts.Common;
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