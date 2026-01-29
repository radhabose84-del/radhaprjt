using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeederGroup;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.Power.FeederGroup.Queries.GetFeederGroup
{
    public class GetFeederGroupQueryHandler : IRequestHandler<GetFeederGroupQuery, ApiResponseDTO<List<FeederGroupDto>>>
    {

       private readonly IFeederGroupQueryRepository _feederGroupQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public  GetFeederGroupQueryHandler( IFeederGroupQueryRepository feederGroupQueryRepository, IMapper mapper, IMediator mediator)
        {
            _feederGroupQueryRepository = feederGroupQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<FeederGroupDto>>> Handle(GetFeederGroupQuery request, CancellationToken cancellationToken)
        {
            
            var (FeederGroup, totalCount) = await _feederGroupQueryRepository.GetAllFeederGroupAsync(request.PageNumber, request.PageSize, request.SearchTerm);            
            var FeederGroupgrouplist = _mapper.Map<List<FeederGroupDto>>(FeederGroup);            

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetFeederGroupQuery",
                actionCode: "Get",        
                actionName: FeederGroup.Count().ToString(),
                details: $"FeederGroup details was fetched",
                module:"FeederGroup"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<FeederGroupDto>> 
            { 
                IsSuccess = true, 
                Message = "Success", 
                Data = FeederGroupgrouplist ,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
            
        }
    }
}