using AutoMapper;
using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces.IState;
using Core.Application.State.Queries.GetStates;
using Core.Domain.Events;
using MediatR;

namespace Core.Application.State.Queries.GetCountries
{
    public class GetStateQueryHandler : IRequestHandler<GetStateQuery, ApiResponseDTO<List<StateDto>>>
    {
        private readonly IStateQueryRepository _stateRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        public GetStateQueryHandler(IStateQueryRepository stateRepository , IMapper mapper, IMediator mediator)
        {
            _stateRepository = stateRepository;  
            _mapper = mapper;
            _mediator = mediator;
        }
        public async Task<ApiResponseDTO<List<StateDto>>> Handle(GetStateQuery request, CancellationToken cancellationToken)
        {
           
            var (states, totalCount)= await _stateRepository.GetAllStatesAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var statesList = _mapper.Map<List<StateDto>>(states);            
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",        
                actionName: "",
                details: $"State details was fetched.",
                module:"State"
            );
            await _mediator.Publish(domainEvent, cancellationToken);    
             return new ApiResponseDTO<List<StateDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = statesList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };       
        }
    }
}