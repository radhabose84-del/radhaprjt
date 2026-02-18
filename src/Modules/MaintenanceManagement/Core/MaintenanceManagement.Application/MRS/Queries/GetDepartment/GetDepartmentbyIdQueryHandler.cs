#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMRS;
using MaintenanceManagement.Application.MRS.Queries.GetDepartment;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MRS.Queries
{
    public class GetDepartmentbyIdQueryHandler : IRequestHandler<GetDepartmentbyIdQuery, List<MDepartmentDto>>
{
    private readonly IMRSQueryRepository _imRSQueryRepository;        
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public GetDepartmentbyIdQueryHandler(IMRSQueryRepository imRSQueryRepository, IMapper mapper, IMediator mediator)
    {
        _imRSQueryRepository = imRSQueryRepository;            
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<List<MDepartmentDto>> Handle(GetDepartmentbyIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _imRSQueryRepository.GetMDepartment(request.OldUnitcode);


        var departmentDtos = _mapper.Map<List<MDepartmentDto>>(result);

        var domainEvent = new AuditLogsDomainEvent(
            actionDetail: "GetById",
            actionCode: "GetDepartmentbyIdQuery",
            actionName: request.OldUnitcode,
            details: $"Department {request.OldUnitcode} was fetched.",
            module: "GetDepartment"
        );

        await _mediator.Publish(domainEvent, cancellationToken);

        return  departmentDtos;
    }
}
}