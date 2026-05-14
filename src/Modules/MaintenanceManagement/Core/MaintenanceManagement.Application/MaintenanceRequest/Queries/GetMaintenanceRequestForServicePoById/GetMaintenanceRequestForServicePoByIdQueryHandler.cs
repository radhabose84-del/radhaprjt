using Contracts.Common;
using Contracts.Dtos.Lookups.Maintenance;
using Contracts.Interfaces.Lookups.Maintenance;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceRequestForServicePoById
{
    public sealed class GetMaintenanceRequestForServicePoByIdQueryHandler
        : IRequestHandler<GetMaintenanceRequestForServicePoByIdQuery, ApiResponseDTO<MaintenanceRequestLookupDto?>>
    {
        private readonly IMaintenanceRequestLookup _lookup;

        public GetMaintenanceRequestForServicePoByIdQueryHandler(IMaintenanceRequestLookup lookup)
        {
            _lookup = lookup;
        }

        public async Task<ApiResponseDTO<MaintenanceRequestLookupDto?>> Handle(
            GetMaintenanceRequestForServicePoByIdQuery request, CancellationToken cancellationToken)
        {
            var dto = await _lookup.GetByIdAsync(request.Id, cancellationToken);

            return new ApiResponseDTO<MaintenanceRequestLookupDto?>
            {
                IsSuccess = dto != null,
                Message = dto != null ? "Success" : "MaintenanceRequest not found.",
                Data = dto
            };
        }
    }
}
