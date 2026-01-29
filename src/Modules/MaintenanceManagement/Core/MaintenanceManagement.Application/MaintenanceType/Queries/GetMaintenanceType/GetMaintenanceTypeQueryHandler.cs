using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceType;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceType.Queries.GetMaintenanceType
{
    public class GetMaintenanceTypeQueryHandler : IRequestHandler<GetMaintenanceTypeQuery, ApiResponseDTO<List<MaintenanceTypeDto>>>
    {
        
        private readonly IMaintenanceTypeQueryRepository _imaintenanceTypeQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetMaintenanceTypeQueryHandler(IMaintenanceTypeQueryRepository imaintenanceTypeQueryRepository, IMapper mapper, IMediator mediator)
        {
            _imaintenanceTypeQueryRepository = imaintenanceTypeQueryRepository;            
            _mapper = mapper;
            _mediator = mediator;   
        }
        public async Task<ApiResponseDTO<List<MaintenanceTypeDto>>> Handle(GetMaintenanceTypeQuery request, CancellationToken cancellationToken)
        {
             var (MaintenanceCategory, totalCount) = await _imaintenanceTypeQueryRepository.GetAllMaintenanceTypeAsync(request.PageNumber, request.PageSize, request.SearchTerm);
               var maintenanceCategoriesgrouplist = _mapper.Map<List<MaintenanceTypeDto>>(MaintenanceCategory);

             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetMaintenanceTypeQuery",
                    actionCode: "Get",        
                    actionName: MaintenanceCategory.Count().ToString(),
                    details: $"MaintenanceType details was fetched.",
                    module:"MaintenanceType"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<MaintenanceTypeDto>> 
            { 
                IsSuccess = true, 
                Message = "Success", 
                Data = maintenanceCategoriesgrouplist ,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
                };
        }
    }
}