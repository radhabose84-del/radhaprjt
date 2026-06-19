using MediatR;
using PurchaseManagement.Application.Arrival.Dto;

namespace PurchaseManagement.Application.Arrival.Queries.GetArrivalPoPrefill
{
    /// <summary>
    /// PO-select prefill for the arrival form: balance qty per item + approved freight (transporter/party
    /// + rate) from the Approved Freight RFQ linked to the PO.
    /// </summary>
    public sealed record GetArrivalPoPrefillQuery(int RawMaterialPOId) : IRequest<ArrivalPoPrefillDto>;
}
