using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IMRS;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MRS.Queries.GetCategory
{
    public class GetCategoryQueryHandler : IRequestHandler<GetCategoryQuery, List<MCategoryDto>>
    {
        private readonly IMRSQueryRepository _imRSQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetCategoryQueryHandler(IMRSQueryRepository imRSQueryRepository, IMapper mapper, IMediator mediator)
        {
        _imRSQueryRepository = imRSQueryRepository;            
        _mapper = mapper;
        _mediator = mediator;
        }

        public async Task<List<MCategoryDto>> Handle(GetCategoryQuery request, CancellationToken cancellationToken)
        {
            var result = await _imRSQueryRepository.GetCategory(request.OldUnitcode);


        var departmentDtos = _mapper.Map<List<MCategoryDto>>(result);

        var domainEvent = new AuditLogsDomainEvent(
            actionDetail: "GetById",
            actionCode: "GetCategoryQuery",
            actionName: request.OldUnitcode,
            details: $"Category {request.OldUnitcode} was fetched.",
            module: "GetCategory"
        );

        await _mediator.Publish(domainEvent, cancellationToken);

        return  departmentDtos;
        }
    }
}