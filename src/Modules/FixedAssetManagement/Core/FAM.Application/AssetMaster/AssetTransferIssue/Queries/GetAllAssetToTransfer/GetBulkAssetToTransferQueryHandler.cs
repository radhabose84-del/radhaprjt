#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetDtlToTransfer;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetBulkAssetToTransfer
{
    public class GetBulkAssetToTransferQueryHandler : IRequestHandler<GetBulkAssetToTransferQuery, ApiResponseDTO<List<GetAssetDetailsToTransferHdrDto>>>
    {

        private readonly IAssetTransferQueryRepository _assetTransferQueryRepository;
        private readonly IMapper _mapper;
        public GetBulkAssetToTransferQueryHandler(IAssetTransferQueryRepository assetTransferQueryRepository, IMapper mapper)
        {
            _assetTransferQueryRepository = assetTransferQueryRepository;
            _mapper = mapper;
        }


            public async Task<ApiResponseDTO<List<GetAssetDetailsToTransferHdrDto>>> Handle(GetBulkAssetToTransferQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.CustodianId))
            {
                return new ApiResponseDTO<List<GetAssetDetailsToTransferHdrDto>>
                {
                    IsSuccess = false,
                    Message = "CustodianId is required.",
                    Data = null
                };
            }

            var asset = await _assetTransferQueryRepository.GetAssetDetailsToTransferByFiltersAsync(request.CustodianId, request.DepartmentId, request.CategoryID);

            var assetList = _mapper.Map<List<GetAssetDetailsToTransferHdrDto>>(asset);  
            return new ApiResponseDTO<List<GetAssetDetailsToTransferHdrDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = assetList,
                TotalCount = assetList.Count
            };
        }

    }
}