#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMRS;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MRS.Queries.GetSubDepartment
{
    public class GetSubDepartmentQueryHandler : IRequestHandler<GetSubDepartmentQuery, List<MSubDepartment>>
    {
        private readonly IMRSQueryRepository _imRSQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetSubDepartmentQueryHandler(IMRSQueryRepository imRSQueryRepository, IMapper mapper, IMediator mediator)
        {
        _imRSQueryRepository = imRSQueryRepository;            
        _mapper = mapper;
        _mediator = mediator;
        }

        public async Task<List<MSubDepartment>> Handle(GetSubDepartmentQuery request, CancellationToken cancellationToken)
        {
        var result = await _imRSQueryRepository.GetSubDepartment(request.OldUnitcode);


        var departmentDtos = _mapper.Map<List<MSubDepartment>>(result);

        var domainEvent = new AuditLogsDomainEvent(
            actionDetail: "GetById",
            actionCode: "GetSubDepartmentQuery",
            actionName: request.OldUnitcode,
            details: $"SubDepartment {request.OldUnitcode} was fetched.",
            module: "GetSubDepartment"
        );

        await _mediator.Publish(domainEvent, cancellationToken);

        return  departmentDtos;
        }
    }
}