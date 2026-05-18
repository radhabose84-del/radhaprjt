using Contracts.Common;
using MaintenanceManagement.Application.ServiceHistory.Dto;
using MediatR;

namespace MaintenanceManagement.Application.ServiceHistory.Queries.GetServiceHistory
{
    public class GetServiceHistoryQuery : IRequest<ApiResponseDTO<List<ServiceHistoryDto>>>
    {
        /// <summary>Filter by machine. At least one of MachineId / AssetId is required.</summary>
        public int? MachineId { get; set; }

        /// <summary>Filter by FixedAssetManagement asset. At least one of MachineId / AssetId is required.</summary>
        public int? AssetId { get; set; }

        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
