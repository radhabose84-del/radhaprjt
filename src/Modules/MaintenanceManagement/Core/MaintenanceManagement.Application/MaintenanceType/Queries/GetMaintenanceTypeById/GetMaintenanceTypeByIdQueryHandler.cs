using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceType;
using MaintenanceManagement.Application.MaintenanceType.Queries.GetMaintenanceType;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceType.Queries.GetMaintenanceTypeById
{
    public class GetMaintenanceTypeByIdQueryHandler : IRequestHandler<GetMaintenanceTypeByIdQuery, MaintenanceTypeDto>
    {
         private readonly IMaintenanceTypeQueryRepository _imaintenanceTypeQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetMaintenanceTypeByIdQueryHandler(IMaintenanceTypeQueryRepository imaintenanceTypeQueryRepository, IMapper mapper, IMediator mediator)
        {
            _imaintenanceTypeQueryRepository = imaintenanceTypeQueryRepository;            
            _mapper = mapper;
            _mediator = mediator;   
        }

        public async Task<MaintenanceTypeDto> Handle(GetMaintenanceTypeByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _imaintenanceTypeQueryRepository.GetByIdAsync(request.Id);
         
            // Map a single entity
            var maintenanceCategory = _mapper.Map<MaintenanceTypeDto>(result);

          //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetById",
                    actionCode: "GetMaintenanceTypeByIdQuery",        
                    actionName: maintenanceCategory.Id.ToString(),
                    details: $"MaintenanceType details {maintenanceCategory.Id} was fetched.",
                    module:"MaintenanceType"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
          return maintenanceCategory;
        }
    }
}