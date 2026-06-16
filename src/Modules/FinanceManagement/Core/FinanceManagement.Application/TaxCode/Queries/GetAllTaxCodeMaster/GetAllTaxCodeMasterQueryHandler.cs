using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetAllTaxCodeMaster
{
    public class GetAllTaxCodeMasterQueryHandler : IRequestHandler<GetAllTaxCodeMasterQuery, ApiResponseDTO<List<TaxCodeMasterDto>>>
    {
        private readonly ITaxCodeQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllTaxCodeMasterQueryHandler(ITaxCodeQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<TaxCodeMasterDto>>> Handle(GetAllTaxCodeMasterQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllTaxCodesAsync(request.PageNumber, request.PageSize, request.SearchTerm, request.CompanyId, request.TaxType);

            var dtos = _mapper.Map<List<TaxCodeMasterDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllTaxCodeMasterQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "TaxCodeMaster details were fetched.",
                module: "TaxCodeMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<TaxCodeMasterDto>>
            {
                IsSuccess = true,
                Message = "Tax Code list retrieved successfully.",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
