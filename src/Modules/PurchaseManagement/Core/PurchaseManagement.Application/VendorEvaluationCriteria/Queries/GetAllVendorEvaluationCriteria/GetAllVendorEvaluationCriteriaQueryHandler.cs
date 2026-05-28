using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationCriteria;
using PurchaseManagement.Application.VendorEvaluationCriteria.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.VendorEvaluationCriteria.Queries.GetAllVendorEvaluationCriteria
{
    public class GetAllVendorEvaluationCriteriaQueryHandler : IRequestHandler<GetAllVendorEvaluationCriteriaQuery, ApiResponseDTO<List<VendorEvaluationCriteriaDto>>>
    {
        private readonly IVendorEvaluationCriteriaQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllVendorEvaluationCriteriaQueryHandler(IVendorEvaluationCriteriaQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<VendorEvaluationCriteriaDto>>> Handle(GetAllVendorEvaluationCriteriaQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var dtos = _mapper.Map<List<VendorEvaluationCriteriaDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllVendorEvaluationCriteriaQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "VendorEvaluationCriteria details were fetched.",
                module: "VendorEvaluationCriteria"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<VendorEvaluationCriteriaDto>>
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
