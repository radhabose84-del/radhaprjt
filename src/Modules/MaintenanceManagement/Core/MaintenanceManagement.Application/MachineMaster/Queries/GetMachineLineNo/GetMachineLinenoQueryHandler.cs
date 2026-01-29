using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace MaintenanceManagement.Application.MachineMaster.Queries.GetMachineLineNo
{
    public class GetMachineLinenoQueryHandler : IRequestHandler<GetMachineLinenoQuery, ApiResponseDTO<List<GetMiscMasterDto>>>
    {
        private readonly IMachineMasterQueryRepository _imachineMasterQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        public GetMachineLinenoQueryHandler(IMachineMasterQueryRepository imachineMasterQueryRepository, IMapper mapper, IMediator mediator)
        {
            _imachineMasterQueryRepository = imachineMasterQueryRepository;            
            _mapper = mapper;
            _mediator = mediator;            
        }


        public async Task<ApiResponseDTO<List<GetMiscMasterDto>>> Handle(GetMachineLinenoQuery request, CancellationToken cancellationToken)
        {
              var machinelinenoTypes = await _imachineMasterQueryRepository.GetMachineLineNoAsync();
              var machinelinenoTypesDtoList = _mapper.Map<List<GetMiscMasterDto>>(machinelinenoTypes);

                return new ApiResponseDTO<List<GetMiscMasterDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = machinelinenoTypesDtoList,
                    TotalCount = machinelinenoTypesDtoList.Count
                };
        }
    }
}