using MediatR;
using PurchaseManagement.Application.Arrival.Dto;

namespace PurchaseManagement.Application.Arrival.Queries.GetArrivalBalanceQty
{
    /// <summary>
    /// Returns the remaining (balance) quantity per item for a Raw Material PO, scoped to the
    /// current unit: PO ordered qty − total arrived qty.
    /// </summary>
    public sealed record GetArrivalBalanceQtyQuery(int RawMaterialPOId)
        : IRequest<IReadOnlyList<ArrivalBalanceQtyDto>>;
}
