using Contracts.Common;
using Contracts.Dtos.Lookups.Maintenance;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceRequestForServicePoById
{
    /// <summary>
    /// Single-record lookup for auto-populating Service PO fields after the user picks an ESR.
    /// </summary>
    public sealed class GetMaintenanceRequestForServicePoByIdQuery
        : IRequest<ApiResponseDTO<MaintenanceRequestLookupDto?>>
    {
        public int Id { get; set; }
    }
}
