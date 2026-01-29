using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetTransferReceipt;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptDetails
{
    public class GetAssetReceiptDetailsQueryHandler : IRequestHandler<GetAssetReceiptDetailsQuery,  ApiResponseDTO<List<AssetReceiptDetailsDto>>>
    {
        private readonly IAssetTransferReceiptQueryRepository _assetTransferReceiptQueryRepository;
        private readonly IMapper _mapper;        
        private readonly IMediator _mediator; 

        public GetAssetReceiptDetailsQueryHandler(IAssetTransferReceiptQueryRepository assetTransferReceiptQueryRepository, IMapper mapper, IMediator mediator)
        {
            _assetTransferReceiptQueryRepository = assetTransferReceiptQueryRepository;
            _mapper = mapper;            
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<AssetReceiptDetailsDto>>> Handle(GetAssetReceiptDetailsQuery request, CancellationToken cancellationToken)
        {
            var (AssetTransferReceipt, totalCount) = await _assetTransferReceiptQueryRepository
                                                .GetAllAssetReceiptDetails(request.PageNumber, request.PageSize, request.SearchTerm, request.FromDate, request.ToDate);
            var assetreceiptlist = _mapper.Map<List<AssetReceiptDetailsDto>>(AssetTransferReceipt);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "Get",        
                actionName: assetreceiptlist.Count.ToString(),
                details: $"Asset Receipt details was fetched.",
                module:"Asset Receipt"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<AssetReceiptDetailsDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = AssetTransferReceipt,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize                
            };  
        }
    }
}