using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetAllTaxAccountLinkage
{
    public class GetAllTaxAccountLinkageQueryHandler : IRequestHandler<GetAllTaxAccountLinkageQuery, ApiResponseDTO<List<TaxAccountLinkageDto>>>
    {
        private readonly ITaxCodeQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllTaxAccountLinkageQueryHandler(ITaxCodeQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<TaxAccountLinkageDto>>> Handle(GetAllTaxAccountLinkageQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllLinkagesAsync(request.PageNumber, request.PageSize, request.SearchTerm, request.CompanyId);

            var dtos = _mapper.Map<List<TaxAccountLinkageDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllTaxAccountLinkageQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "TaxAccountLinkage details were fetched.",
                module: "TaxAccountLinkage"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<TaxAccountLinkageDto>>
            {
                IsSuccess = true,
                Message = "Tax-account linkage list retrieved successfully.",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
