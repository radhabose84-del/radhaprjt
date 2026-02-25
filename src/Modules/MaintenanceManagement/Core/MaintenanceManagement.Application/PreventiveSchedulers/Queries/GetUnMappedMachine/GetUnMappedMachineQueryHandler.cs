using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MediatR;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetUnMappedMachine
{
    public class GetUnMappedMachineQueryHandler : IRequestHandler<GetUnMappedMachineQuery, ApiResponseDTO<List<UnMappedMachineDto>>>
    {
        private readonly IPreventiveSchedulerQuery _preventiveSchedulerQuery;
         private readonly IMapper _mapper;
        public GetUnMappedMachineQueryHandler(IPreventiveSchedulerQuery preventiveSchedulerQuery, IMapper mapper)
        {
            _preventiveSchedulerQuery = preventiveSchedulerQuery;
            _mapper = mapper;
        }
        public async Task<ApiResponseDTO<List<UnMappedMachineDto>>> Handle(GetUnMappedMachineQuery request, CancellationToken cancellationToken)
        {
            var unMappedMachines = await _preventiveSchedulerQuery.GetUnMappedMachineIdByCode(request.Id);

            var unMappedMachinesList = _mapper.Map<List<UnMappedMachineDto>>(unMappedMachines);

            return new ApiResponseDTO<List<UnMappedMachineDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = unMappedMachinesList
            };
        }
    }
}