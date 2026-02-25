using Contracts.Common;
using MediatR;

namespace FAM.Application.ExcelImport.PhysicalStockVerification
{
    public class ImportAssetAuditCommand  : IRequest<ApiResponseDTO<bool>>
    {
        public ImportAssetAuditDto ImportAssetAuditDto { get; set; }

        public ImportAssetAuditCommand(ImportAssetAuditDto importAssetAuditDto)
        {
            ImportAssetAuditDto = importAssetAuditDto;
        }
    }
}