using MediatR;
using PurchaseManagement.Application.RawMaterialPO.Dto;

namespace PurchaseManagement.Application.RawMaterialPO.Queries.GetRawMaterialPOById
{
    public class GetRawMaterialPOByIdQuery : IRequest<RawMaterialPODto?>
    {
        public int Id { get; set; }
    }
}
