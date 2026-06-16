using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetAllGstrSectionMapping
{
    public class GetAllGstrSectionMappingQueryHandler : IRequestHandler<GetAllGstrSectionMappingQuery, ApiResponseDTO<List<GstrSectionMappingDto>>>
    {
        private readonly ITaxCodeQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllGstrSectionMappingQueryHandler(ITaxCodeQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<GstrSectionMappingDto>>> Handle(GetAllGstrSectionMappingQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllGstrMappingsAsync(request.PageNumber, request.PageSize, request.SearchTerm, request.CompanyId);

            var dtos = _mapper.Map<List<GstrSectionMappingDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllGstrSectionMappingQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "GstrSectionMapping details were fetched.",
                module: "GstrSectionMapping"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<GstrSectionMappingDto>>
            {
                IsSuccess = true,
                Message = "GSTR section mapping list retrieved successfully.",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
