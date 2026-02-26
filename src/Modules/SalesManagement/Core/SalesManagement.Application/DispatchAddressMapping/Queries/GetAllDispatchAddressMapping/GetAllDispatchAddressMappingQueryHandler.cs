using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMapping;
using SalesManagement.Application.DispatchAddressMapping.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DispatchAddressMapping.Queries.GetAllDispatchAddressMapping
{
    public class GetAllDispatchAddressMappingQueryHandler : IRequestHandler<GetAllDispatchAddressMappingQuery, ApiResponseDTO<List<DispatchAddressMappingDto>>>
    {
        private readonly IDispatchAddressMappingQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllDispatchAddressMappingQueryHandler(
            IDispatchAddressMappingQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<DispatchAddressMappingDto>>> Handle(GetAllDispatchAddressMappingQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm, request.PartyId);

            var dtos = _mapper.Map<List<DispatchAddressMappingDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllDispatchAddressMappingQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "DispatchAddressMapping details were fetched.",
                module: "DispatchAddressMapping"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<DispatchAddressMappingDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
