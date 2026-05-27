using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces.Lookups.Inventory;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQualityParameter;
using QCManagement.Application.QualityParameter.Dto;
using QCManagement.Domain.Events;

namespace QCManagement.Application.QualityParameter.Queries.GetAllQualityParameter
{
    public class GetAllQualityParameterQueryHandler : IRequestHandler<GetAllQualityParameterQuery, ApiResponseDTO<List<QualityParameterDto>>>
    {
        private readonly IQualityParameterQueryRepository _queryRepository;
        private readonly IUOMLookup _uomLookup;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllQualityParameterQueryHandler(
            IQualityParameterQueryRepository queryRepository,
            IUOMLookup uomLookup,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _uomLookup = uomLookup;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<QualityParameterDto>>> Handle(GetAllQualityParameterQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm, request.ParameterGroupId);

            var dtos = _mapper.Map<List<QualityParameterDto>>(data);

            // Populate UnitCode/UnitName from cross-module IUOMLookup (Unit of Measure)
            var unitIds = dtos.Where(d => d.UnitId.HasValue).Select(d => d.UnitId!.Value).Distinct().ToList();
            if (unitIds.Count > 0)
            {
                var uoms = await _uomLookup.GetByIdsAsync(unitIds, cancellationToken);
                var uomMap = uoms.ToDictionary(u => u.Id);
                foreach (var dto in dtos.Where(d => d.UnitId.HasValue))
                {
                    if (uomMap.TryGetValue(dto.UnitId!.Value, out var uom))
                    {
                        dto.UnitCode = uom.Code;
                        dto.UnitName = uom.UOMName;
                    }
                }
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllQualityParameterQuery",
                actionCode: "Get",
                actionName: dtos.Count.ToString(),
                details: "QualityParameter details were fetched.",
                module: "QualityParameter"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<QualityParameterDto>>
            {
                IsSuccess = true,
                Message = "Quality Parameter list retrieved successfully.",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
