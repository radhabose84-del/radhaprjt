using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceCategory;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceCategory.Queries.GetMaintenanceCategory
{
    public class GetMaintenanceCategoryQueryHandler : IRequestHandler<GetMaintenanceCategoryQuery,ApiResponseDTO<List<MaintenanceCategoryDto>>>
    {
        private readonly IMaintenanceCategoryQueryRepository _iMaintenanceCategoryQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetMaintenanceCategoryQueryHandler(IMaintenanceCategoryQueryRepository iMaintenanceCategoryQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iMaintenanceCategoryQueryRepository = iMaintenanceCategoryQueryRepository;            
            _mapper = mapper;
            _mediator = mediator;   
        }

        public async Task<ApiResponseDTO<List<MaintenanceCategoryDto>>> Handle(GetMaintenanceCategoryQuery request, CancellationToken cancellationToken)
        {
             var (MaintenanceCategory, totalCount) = await _iMaintenanceCategoryQueryRepository.GetAllMaintenanceCategoryAsync(request.PageNumber, request.PageSize, request.SearchTerm);
               var maintenanceCategoriesgrouplist = _mapper.Map<List<MaintenanceCategoryDto>>(MaintenanceCategory);

             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetMaintenanceCategoryQuery",
                    actionCode: "Get",        
                    actionName: MaintenanceCategory.Count().ToString(),
                    details: $"MaintenanceCategory details was fetched.",
                    module:"MaintenanceCategory"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<MaintenanceCategoryDto>> 
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