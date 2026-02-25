using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.ShiftMasters.Queries.GetShiftMasterAutoComplete
{
    public class GetShiftMasterAutoCompleteQuery : IRequest<ApiResponseDTO<List<ShiftMasterAutoCompleteDTO>>>
    {
        public string SearchPattern { get; set; } = default!;
    }
}