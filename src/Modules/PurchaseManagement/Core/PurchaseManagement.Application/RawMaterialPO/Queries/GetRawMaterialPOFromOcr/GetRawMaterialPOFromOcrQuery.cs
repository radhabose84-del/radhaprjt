using MediatR;
using PurchaseManagement.Application.RawMaterialPO.Dto;

namespace PurchaseManagement.Application.RawMaterialPO.Queries.GetRawMaterialPOFromOcr
{
    public class GetRawMaterialPOFromOcrQuery : IRequest<OcrConversionDto?>
    {
        public int OcrId { get; set; }
    }
}
