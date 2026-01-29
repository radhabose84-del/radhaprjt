using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IDepreciationDetail;
using FAM.Application.DepreciationDetail.Queries.GetDepreciationDetail;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.DepreciationDetail.Queries.GetDepreciationAbstract
{
    public class GetDepreciationAbstractQueryHandler  : IRequestHandler<GetDepreciationAbstractQuery, List<DepreciationAbstractDto>>
    {
        private readonly IDepreciationDetailQueryRepository _depreciationDetailRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        public GetDepreciationAbstractQueryHandler(IDepreciationDetailQueryRepository depreciationDetailRepository , IMapper mapper, IMediator mediator)
        {
            _depreciationDetailRepository = depreciationDetailRepository;
            _mapper = mapper;
            _mediator = mediator;
        }        
        public async Task<List<DepreciationAbstractDto>> Handle(GetDepreciationAbstractQuery request, CancellationToken cancellationToken)
        {
            var depreciationAbstract = await _depreciationDetailRepository.GetDepreciationAbstractAsync(request.companyId,request.unitId, request.finYearId, request.startDate,request.endDate, request.depreciationPeriod,request.depreciationType);
            var depreciationAbstractList = _mapper.Map<List<DepreciationAbstractDto>>(depreciationAbstract);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",        
                actionName: "",
                details: $"Depreciation Abstract details was fetched.",
                module:"Asset Specification"
            );
            
            await _mediator.Publish(domainEvent, cancellationToken);
            return depreciationAbstractList;            
        }
    }
  
}