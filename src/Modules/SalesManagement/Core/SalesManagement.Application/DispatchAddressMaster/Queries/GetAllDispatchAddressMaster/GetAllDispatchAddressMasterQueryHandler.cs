using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMaster;
using SalesManagement.Application.DispatchAddressMaster.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DispatchAddressMaster.Queries.GetAllDispatchAddressMaster
{
    public class GetAllDispatchAddressMasterQueryHandler : IRequestHandler<GetAllDispatchAddressMasterQuery, ApiResponseDTO<List<DispatchAddressMasterDto>>>
    {
        private readonly IDispatchAddressMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllDispatchAddressMasterQueryHandler(IDispatchAddressMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<DispatchAddressMasterDto>>> Handle(GetAllDispatchAddressMasterQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var dtos = _mapper.Map<List<DispatchAddressMasterDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllDispatchAddressMasterQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "DispatchAddressMaster details were fetched.",
                module: "DispatchAddressMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<DispatchAddressMasterDto>>
            {
                IsSuccess = true,
                Message = "Dispatch Address Master records retrieved successfully.",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
