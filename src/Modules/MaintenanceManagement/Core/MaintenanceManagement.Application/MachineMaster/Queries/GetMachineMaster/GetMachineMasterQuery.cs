using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMaster
{
    public class GetMachineMasterQuery : IRequest<ApiResponseDTO<List<MachineMasterDto>>>
    {
        public string? SearchTerm { get; set; }
    }
}