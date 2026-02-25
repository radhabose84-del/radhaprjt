using MediatR;

namespace PurchaseManagement.Application.MRS.Queries.GetParentWarehouse
{
    public class GetParentWarehouseQuery : IRequest<GetParentWarehouseDto>
    {
        public int WarehouseId { get; set; }
    }
}