#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using UserManagement.Application.Common.Interfaces.IDepartment;
using UserManagement.Application.Common.Interfaces.IDepartmentGroup;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;

namespace UserManagement.Application.DepartmentGroup.Queries.GetDepartmentGroupById
{
  public class GetDepartmentGroupByIdQueryHandler : IRequestHandler<GetDepartmentGroupByIdQuery, DepartmentGroupByIdDto>
  {
    private readonly IDepartmentGroupQueryRepository _departmentGroupRepository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public GetDepartmentGroupByIdQueryHandler(IDepartmentGroupQueryRepository departmentGroupRepository, IMapper mapper, IMediator mediator)
    {
      _departmentGroupRepository = departmentGroupRepository;
      _mapper = mapper;
      _mediator = mediator;

    }

        public async Task<DepartmentGroupByIdDto> Handle(GetDepartmentGroupByIdQuery request, CancellationToken cancellationToken)
        {
               var departmentGroup = await _departmentGroupRepository.GetDepartmentGroupByIdAsync(request.Id);
                    
                    if (departmentGroup == null)
                    {
                       throw new ValidationException("epartment not found.");
                     
                    }
            

              var deptDto = _mapper.Map<DepartmentGroupByIdDto>(departmentGroup);
 //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetById",
                    actionCode: deptDto.Id.ToString(),        
                    actionName: deptDto.DepartmentGroupName,                
                    details: $"Department '{deptDto.DepartmentGroupName}' was created. DepartmentCode: {deptDto.Id}",
                    module:"Department"
                );

                await _mediator.Publish(domainEvent, cancellationToken);
            return deptDto;

               
        }
    }
}