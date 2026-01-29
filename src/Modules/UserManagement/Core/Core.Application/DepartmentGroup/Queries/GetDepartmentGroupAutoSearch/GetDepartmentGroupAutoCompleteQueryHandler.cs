using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces.IDepartmentGroup;
using Core.Domain.Events;
using FluentValidation;
using MediatR;

namespace Core.Application.DepartmentGroup.Queries.GetDepartmentGroupAutoSearch
{
    public class GetDepartmentGroupAutoCompleteQueryHandler   : IRequestHandler<GetDepartmentGroupAutoCompleteQuery, List<DepartmentGroupAutoCompleteDto>>
    {
        private readonly IDepartmentGroupQueryRepository _departmentGroupRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetDepartmentGroupAutoCompleteQueryHandler(IDepartmentGroupQueryRepository departmentGroupQueryRepository, IMapper mapper, IMediator mediator)
        {
            _departmentGroupRepository = departmentGroupQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }
        
         public async Task<List<DepartmentGroupAutoCompleteDto>> Handle(GetDepartmentGroupAutoCompleteQuery request, CancellationToken cancellationToken)
        {          
            var result = await _departmentGroupRepository.GetAllDepartmentGroupAsync(request.SearchPattern ?? string.Empty);
            if (result is null || result.Count is 0)
            {
                throw new ValidationException("No DepartmentGroup found matching the search pattern.");
              
            }
            var stateDto = _mapper.Map<List<DepartmentGroupAutoCompleteDto>>(result);
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAutoComplete",
                actionCode:"",        
                actionName: request.SearchPattern ?? string.Empty,                
                details: $"DepartmentGroup '{request.SearchPattern}' was searched",
                module:"DepartmentGroup"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return stateDto;
        }


    }
}