using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Application.DispatchAdvice.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DispatchAdvice.Queries.GetAllDispatchAdvice
{
    public class GetAllDispatchAdviceQueryHandler : IRequestHandler<GetAllDispatchAdviceQuery, ApiResponseDTO<List<DispatchAdviceHeaderDto>>>
    {
        private readonly IDispatchAdviceQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllDispatchAdviceQueryHandler(
            IDispatchAdviceQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<DispatchAdviceHeaderDto>>> Handle(GetAllDispatchAdviceQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllDispatchAdviceQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Dispatch Advice details were fetched.",
                module: "DispatchAdvice");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<DispatchAdviceHeaderDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
