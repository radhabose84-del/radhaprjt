using Contracts.Common;
using Contracts.Dtos.Lookups.Maintenance;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceRequestsForServicePo
{
    /// <summary>
    /// Lists External Maintenance Requests eligible for linking from a Service PO.
    /// Status filter: Open, In-Progress, PartiallyConverted.
    /// </summary>
    public sealed class GetMaintenanceRequestsForServicePoQuery
        : IRequest<ApiResponseDTO<List<MaintenanceRequestLookupDto>>>
    {
        public string? SearchTerm { get; set; }
    }
}
