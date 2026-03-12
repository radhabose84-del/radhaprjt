using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IProcurementType;
using InventoryManagement.Application.ProcurementType.Dto;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.ProcurementType.Queries.GetAllProcurementType
{
    public class GetAllProcurementTypeQueryHandler : IRequestHandler<GetAllProcurementTypeQuery, ApiResponseDTO<List<ProcurementTypeDto>>>
    {
        private readonly IProcurementTypeQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllProcurementTypeQueryHandler(IProcurementTypeQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<ProcurementTypeDto>>> Handle(GetAllProcurementTypeQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var dtos = _mapper.Map<List<ProcurementTypeDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllProcurementTypeQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "ProcurementType details were fetched.",
                module: "ProcurementType"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<ProcurementTypeDto>>
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
