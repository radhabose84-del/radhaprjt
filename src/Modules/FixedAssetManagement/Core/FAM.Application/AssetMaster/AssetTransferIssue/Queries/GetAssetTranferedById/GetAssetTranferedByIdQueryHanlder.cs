using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetTransfered;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetTranferedById
{
   public class  GetAssetTranferedByIdQueryHanlder  : IRequestHandler<GetAssetTranferedByIdQuery, AssetTransferJsonDto>

    {
       private readonly IAssetTransferQueryRepository _assetTransferQueryRepository;       


       // ✅ Constructor
        public GetAssetTranferedByIdQueryHanlder ( IAssetTransferQueryRepository assetTransferQueryRepository)
        {
            _assetTransferQueryRepository = assetTransferQueryRepository;
        }
         // ✅ Handle Method
        public async Task<AssetTransferJsonDto> Handle(GetAssetTranferedByIdQuery request, CancellationToken cancellationToken)
        {
            var assetTransfer = await _assetTransferQueryRepository.GetAssetTransferByIdAsync(request.AssetTransferId);

            if (assetTransfer == null)
            {
                throw new ValidationException($"Asset Transfer Issue with ID {request.AssetTransferId} not found.");
             
            }
                return assetTransfer;            
        }
    }
}


