#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceCategory.Queries.GetMaintenanceCategory;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceCategory.Queries.GetMaintenanceCategoryAutoComplete
{
    public class GetMaintenanceCategoryAutoCompleteQueryHandler : IRequestHandler<GetMaintenanceCategoryAutoCompleteQuery,List<MaintenanceCategoryAutoCompleteDto>>
    {
         private readonly IMaintenanceCategoryQueryRepository _iMaintenanceCategoryQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetMaintenanceCategoryAutoCompleteQueryHandler(IMaintenanceCategoryQueryRepository iMaintenanceCategoryQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iMaintenanceCategoryQueryRepository = iMaintenanceCategoryQueryRepository;            
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<List<MaintenanceCategoryAutoCompleteDto>> Handle(GetMaintenanceCategoryAutoCompleteQuery request, CancellationToken cancellationToken)
        {
             var result = await _iMaintenanceCategoryQueryRepository.GetMaintenanceCategoryAsync(request.SearchPattern);
            var maintenanceCategories = _mapper.Map<List<MaintenanceCategoryAutoCompleteDto>>(result);
             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "GetMaintenanceCategoryAutoCompleteQuery",        
                    actionName: maintenanceCategories.Count.ToString(),
                    details: $"MaintenanceCategory details was fetched.",
                    module:"MaintenanceCategory"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return  maintenanceCategories;
        }
    }
}