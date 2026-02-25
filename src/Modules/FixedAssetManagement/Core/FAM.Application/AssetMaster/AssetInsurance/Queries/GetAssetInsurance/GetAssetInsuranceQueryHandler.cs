using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetInsurance;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetInsurance.Queries.GetAssetInsurance
{
    public class GetAssetInsuranceQueryHandler : IRequestHandler<GetAssetInsuranceQuery, ApiResponseDTO<List<GetAssetInsuranceDto>>>
    {
         private readonly IAssetInsuranceQueryRepository  _assetInsuranceQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

         public GetAssetInsuranceQueryHandler(IAssetInsuranceQueryRepository  assetInsuranceQueryRepository , IMapper mapper, IMediator mediator)
        {
            _assetInsuranceQueryRepository = assetInsuranceQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        } 

        public  async Task<ApiResponseDTO<List<GetAssetInsuranceDto>>> Handle(GetAssetInsuranceQuery request, CancellationToken cancellationToken)
        
        {
           // var (assetInsurance, totalCount) = await _assetInsuranceQueryRepository.GetAllAssetInsuranceAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var (assetInsurance, totalCount)  = await _assetInsuranceQueryRepository.GetAllAssetInsuranceAsync(request.PageNumber, request.PageSize, request.SearchTerm);
          //  var totalCount = assetInsurance.Count;
            var assetinsuranceList = _mapper.Map<List<GetAssetInsuranceDto>>(assetInsurance);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",        
                actionName: "",
                details: $"Asset Insurance details was fetched.",
                module:"Asset Insurance"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<GetAssetInsuranceDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = assetinsuranceList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
                

            };          
        }

       
       
    }
}