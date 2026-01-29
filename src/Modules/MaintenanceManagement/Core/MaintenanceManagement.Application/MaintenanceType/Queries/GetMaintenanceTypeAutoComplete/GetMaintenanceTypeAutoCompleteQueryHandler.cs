using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceType;
using MaintenanceManagement.Application.MaintenanceType.Queries.GetMaintenanceType;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceType.Queries.GetMaintenanceTypeAutoComplete
{
    public class GetMaintenanceTypeAutoCompleteQueryHandler : IRequestHandler<GetMaintenanceTypeAutoCompleteQuery, List<MaintenanceTypeAutoCompleteDto>>
    {
         private readonly IMaintenanceTypeQueryRepository _imaintenanceTypeQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetMaintenanceTypeAutoCompleteQueryHandler(IMaintenanceTypeQueryRepository imaintenanceTypeQueryRepository, IMapper mapper, IMediator mediator)
        {
            _imaintenanceTypeQueryRepository = imaintenanceTypeQueryRepository;            
            _mapper = mapper;
            _mediator = mediator;   
        }
        public async Task<List<MaintenanceTypeAutoCompleteDto>> Handle(GetMaintenanceTypeAutoCompleteQuery request, CancellationToken cancellationToken)
        {
             var result = await _imaintenanceTypeQueryRepository.GetMaintenanceTypeAsync(request.SearchPattern);
            var maintenanceCategories = _mapper.Map<List<MaintenanceTypeAutoCompleteDto>>(result);
             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "GetMaintenanceTypeAutoCompleteQuery",        
                    actionName: maintenanceCategories.Count.ToString(),
                    details: $"MaintenanceCategory details was fetched.",
                    module:"MaintenanceCategory"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return maintenanceCategories;
        }
    }
}