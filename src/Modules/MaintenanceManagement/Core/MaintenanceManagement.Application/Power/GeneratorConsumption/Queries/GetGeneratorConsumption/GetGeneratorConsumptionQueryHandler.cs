using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.Power.IGeneratorConsumption;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.Power.GeneratorConsumption.Queries.GetGeneratorConsumption
{
    public class GetGeneratorConsumptionQueryHandler : IRequestHandler<GetGeneratorConsumptionQuery, ApiResponseDTO<List<GetGeneratorConsumptionDto>>>
    {
        private readonly IGeneratorConsumptionQueryRepository _generatorConsumptionQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetGeneratorConsumptionQueryHandler(IGeneratorConsumptionQueryRepository generatorConsumptionQueryRepository, IMapper mapper, IMediator mediator)
        {
            _generatorConsumptionQueryRepository = generatorConsumptionQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<GetGeneratorConsumptionDto>>> Handle(GetGeneratorConsumptionQuery request, CancellationToken cancellationToken)
        {
            var (PowerConsumption, totalCount) = await _generatorConsumptionQueryRepository.GetAllGeneratorConsumptionAsync(request.PageNumber, request.PageSize, request.SearchTerm);            
            var powerConsumptionGroupgrouplist = _mapper.Map<List<GetGeneratorConsumptionDto>>(PowerConsumption);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetGeneratorConsumptionQuery",
                actionCode: "Get",        
                actionName: PowerConsumption.Count().ToString(),
                details: $"GeneratorConsumption details was fetched",
                module:"GeneratorConsumption"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<GetGeneratorConsumptionDto>> 
            { 
                IsSuccess = true, 
                Message = "Success", 
                Data = powerConsumptionGroupgrouplist ,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
        }

        

}