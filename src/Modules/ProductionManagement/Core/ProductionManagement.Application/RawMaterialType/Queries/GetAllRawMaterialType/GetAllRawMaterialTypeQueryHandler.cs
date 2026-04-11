using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IRawMaterialType;
using ProductionManagement.Application.RawMaterialType.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.RawMaterialType.Queries.GetAllRawMaterialType
{
    public class GetAllRawMaterialTypeQueryHandler : IRequestHandler<GetAllRawMaterialTypeQuery, ApiResponseDTO<List<RawMaterialTypeDto>>>
    {
        private readonly IRawMaterialTypeQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllRawMaterialTypeQueryHandler(
            IRawMaterialTypeQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<RawMaterialTypeDto>>> Handle(GetAllRawMaterialTypeQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var dtos = _mapper.Map<List<RawMaterialTypeDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllRawMaterialTypeQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Raw Material Type details were fetched.",
                module: "RawMaterialType"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<RawMaterialTypeDto>>
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
