using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using FAM.Application.MiscMaster.Queries.GetMiscMaster;
using MassTransit.Mediator;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetTransferType
{
    public class GetTransferTypeQueryHandler   : IRequestHandler<GetTransferTypeQuery, ApiResponseDTO<List<GetMiscMasterDto>>>
    {
        private readonly IAssetTransferQueryRepository _assetTransferQueryRepository;
        private readonly IMapper _mapper;        
     
        
        public GetTransferTypeQueryHandler( IAssetTransferQueryRepository assetTransferQueryRepository, IMapper mapper)
        {
            _assetTransferQueryRepository = assetTransferQueryRepository;
            _mapper = mapper;
                                 
        }

        public async Task<ApiResponseDTO<List<GetMiscMasterDto>>> Handle(GetTransferTypeQuery request, CancellationToken cancellationToken)
        {
              var disposalTypes = await _assetTransferQueryRepository.GetTransferTypeAsync();
             var disposalTypeDtoList = _mapper.Map<List<GetMiscMasterDto>>(disposalTypes);
            return new ApiResponseDTO<List<GetMiscMasterDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = disposalTypeDtoList,
                TotalCount = disposalTypeDtoList.Count
            };
        }
        
    }
}