using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MachineMaster.Queries.GetMachineNoDepartmentbyId
{
    public class GetMachineNoDepartmentbyIdQueryHandler : IRequestHandler<GetMachineNoDepartmentbyIdQuery, List<GetMachineNoDepartmentbyIdDto>>
    {
        private readonly IMachineMasterQueryRepository _imachineMasterQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetMachineNoDepartmentbyIdQueryHandler(IMachineMasterQueryRepository imachineMasterQueryRepository, IMapper mapper, IMediator mediator)
        {
            _imachineMasterQueryRepository = imachineMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
          
        }
        public async Task<List<GetMachineNoDepartmentbyIdDto>> Handle(GetMachineNoDepartmentbyIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _imachineMasterQueryRepository.GetMachineNoDepartmentAsync(request.DepartmentId);
            var machineMasters = _mapper.Map<List<GetMachineNoDepartmentbyIdDto>>(result);
             //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "GetMachineNoDepartmentbyIdQuery",        
                    actionName: machineMasters.Count.ToString(),
                    details: $"MachineMaster details was fetched.",
                    module:"MachineMaster"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return machineMasters;
        }
    }
}