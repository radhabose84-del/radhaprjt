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

namespace MaintenanceManagement.Application.MRS.Queries.GetSubCostCenter
{
    public class GetSubCostCenterQueryHandler : IRequestHandler<GetSubCostCenterQuery, List<MSubCostCenterDto>>
    {
         private readonly IMRSQueryRepository _imRSQueryRepository;        
         private readonly IMapper _mapper;
         private readonly IMediator _mediator;
          public GetSubCostCenterQueryHandler(IMRSQueryRepository imRSQueryRepository, IMapper mapper, IMediator mediator)
         {
                _imRSQueryRepository = imRSQueryRepository;            
                _mapper = mapper;
                _mediator = mediator;
         }

        public async Task<List<MSubCostCenterDto>> Handle(GetSubCostCenterQuery request, CancellationToken cancellationToken)
        {
            var result = await _imRSQueryRepository.GetSubCostCenter(request.OldUnitcode);


        var departmentDtos = _mapper.Map<List<MSubCostCenterDto>>(result);

        var domainEvent = new AuditLogsDomainEvent(
            actionDetail: "GetById",
            actionCode: "GetSubCostCenterQuery",
            actionName: request.OldUnitcode,
            details: $"SubCost Center {request.OldUnitcode} was fetched.",
            module: "GetSubcostCenter"
        );

        await _mediator.Publish(domainEvent, cancellationToken);

        return departmentDtos;
        }
    }
}