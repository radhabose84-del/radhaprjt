using Contracts.Common;
using Contracts.Dtos.Lookups.Maintenance;
using Contracts.Interfaces.Lookups.Maintenance;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceRequestsForServicePo
{
    public sealed class GetMaintenanceRequestsForServicePoQueryHandler
        : IRequestHandler<GetMaintenanceRequestsForServicePoQuery, ApiResponseDTO<List<MaintenanceRequestLookupDto>>>
    {
        private readonly IMaintenanceRequestLookup _lookup;

        public GetMaintenanceRequestsForServicePoQueryHandler(IMaintenanceRequestLookup lookup)
        {
            _lookup = lookup;
        }

        public async Task<ApiResponseDTO<List<MaintenanceRequestLookupDto>>> Handle(
            GetMaintenanceRequestsForServicePoQuery request, CancellationToken cancellationToken)
        {
            var rows = await _lookup.GetAvailableForServicePoAsync(request.SearchTerm, cancellationToken);

            return new ApiResponseDTO<List<MaintenanceRequestLookupDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = rows.ToList(),
                TotalCount = rows.Count
            };
        }
    }
}
