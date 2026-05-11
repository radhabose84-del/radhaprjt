using Contracts.Common;
using MediatR;
using SalesManagement.Application.DeliveryChallan.Dto;

namespace SalesManagement.Application.DeliveryChallan.Queries.GetStandaloneEwbDocument
{
    // Returns a print-ready view of the standalone EWB associated with the given DC.
    // Used for the "View/Print e-Waybill" page on the DC detail screen.
    public sealed record GetStandaloneEwbDocumentQuery(int DeliveryChallanId)
        : IRequest<ApiResponseDTO<StandaloneEwbDocumentDto>>;
}
