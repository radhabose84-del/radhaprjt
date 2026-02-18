
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOderDropdown
{
    public class GetWorkOderDropdownQuery  : IRequest<ApiResponseDTO<List<GetWorkOderDropdownDto>>> 
    {
        
    }
}