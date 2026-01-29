using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IDepreciationDetail;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.DepreciationDetail.Queries.GetDepreciationDetail
{
    public class GetDepreciationDetailQueryHandler : IRequestHandler<GetDepreciationDetailQuery, ApiResponseDTO<List<DepreciationDto>>>
    {
        private readonly IDepreciationDetailQueryRepository _depreciationDetailRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        public GetDepreciationDetailQueryHandler(IDepreciationDetailQueryRepository depreciationDetailRepository , IMapper mapper, IMediator mediator)
        {
            _depreciationDetailRepository = depreciationDetailRepository;
            _mapper = mapper;
            _mediator = mediator;
        }        
        public async Task<ApiResponseDTO<List<DepreciationDto>>> Handle(GetDepreciationDetailQuery request, CancellationToken cancellationToken)
        {
            var (assetSpecification, totalCount,isSuccess,errorMsg) = await _depreciationDetailRepository.CalculateDepreciationAsync(request.companyId,request.unitId, request.finYearId, request.startDate,request.endDate,request.depreciationType,request.PageNumber, request.PageSize, request.SearchTerm,request.depreciationPeriod);
            var assetSpecificationList = _mapper.Map<List<DepreciationDto>>(assetSpecification);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",        
                actionName: "",
                details: $"Asset Specification details was fetched.",
                module:"Asset Specification"
            );
            
            await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<DepreciationDto>>
            {
                IsSuccess = true,
                Message = isSuccess ? "Success" : $"Depreciation calculation failed: {errorMsg}",
                Data = assetSpecificationList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };            
        }
    }
  
}