using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IPackType;
using SalesManagement.Application.PackType.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.PackType.Queries.GetAllPackType
{
    public class GetAllPackTypeQueryHandler : IRequestHandler<GetAllPackTypeQuery, ApiResponseDTO<List<PackTypeDto>>>
    {
        private readonly IPackTypeQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllPackTypeQueryHandler(IPackTypeQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<PackTypeDto>>> Handle(GetAllPackTypeQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var packTypeDtos = _mapper.Map<List<PackTypeDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllPackTypeQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "PackType details were fetched.",
                module: "PackType"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<PackTypeDto>>
            {
                IsSuccess = true,
                Message = "PackTypes retrieved successfully.",
                Data = packTypeDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
