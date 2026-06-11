using MediatR;
using PurchaseManagement.Application.Arrival.Dto;

namespace PurchaseManagement.Application.Arrival.Queries.GetArrivalLastLotNo
{
    /// <summary>Returns the current unit's last lot number (latest arrival), or null when none exists.</summary>
    public sealed record GetArrivalLastLotNoQuery : IRequest<ArrivalLastLotNoDto?>;
}
