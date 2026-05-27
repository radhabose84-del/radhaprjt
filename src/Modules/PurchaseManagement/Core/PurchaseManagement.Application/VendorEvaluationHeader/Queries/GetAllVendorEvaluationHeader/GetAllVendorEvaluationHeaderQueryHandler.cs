using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.VendorEvaluationHeader.Queries.GetAllVendorEvaluationHeader
{
    public class GetAllVendorEvaluationHeaderQueryHandler : IRequestHandler<GetAllVendorEvaluationHeaderQuery, ApiResponseDTO<List<VendorEvaluationHeaderDto>>>
    {
        private readonly IVendorEvaluationHeaderQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllVendorEvaluationHeaderQueryHandler(IVendorEvaluationHeaderQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<VendorEvaluationHeaderDto>>> Handle(GetAllVendorEvaluationHeaderQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var dtos = _mapper.Map<List<VendorEvaluationHeaderDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllVendorEvaluationHeaderQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "VendorEvaluationHeader details were fetched.",
                module: "VendorEvaluationHeader"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<VendorEvaluationHeaderDto>>
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
