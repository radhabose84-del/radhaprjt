using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptDetailsById;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetTransferReceipt;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetRecieptDtlPending
{
    public class GetAssetRecieptDtlPendingQueryHandler : IRequestHandler<GetAssetRecieptDtlPendingQuery, AssetTrasnferReceiptHdrPendingDto>
    {
        
        private readonly IAssetTransferReceiptQueryRepository _assetTransferReceiptQueryRepository;
        private readonly IMapper _mapper;        
        private readonly IMediator _mediator; 

        public GetAssetRecieptDtlPendingQueryHandler(IAssetTransferReceiptQueryRepository assetTransferReceiptQueryRepository, IMapper mapper, IMediator mediator)
        {
            _assetTransferReceiptQueryRepository = assetTransferReceiptQueryRepository;
            _mapper = mapper;            
            _mediator = mediator;   
        }

        public async Task<AssetTrasnferReceiptHdrPendingDto> Handle(GetAssetRecieptDtlPendingQuery request, CancellationToken cancellationToken)
        {
             var assetTransfer = await _assetTransferReceiptQueryRepository.GetAssetTransferByIdAsync(request.AssetTransferId);

            if (assetTransfer == null)
            {
                throw new ValidationException($"Asset Transfer Issue with ID {request.AssetTransferId} not found.");
             
            }
                return  assetTransfer;   
        }
    }
}