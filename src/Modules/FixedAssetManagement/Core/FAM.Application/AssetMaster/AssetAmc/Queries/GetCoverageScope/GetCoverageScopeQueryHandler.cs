using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetMaster.AssetAmc.Queries.GetRenewStatus;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetAmc;
using FAM.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace FAM.Application.AssetMaster.AssetAmc.Queries.GetCoverageScope
{
    public class GetCoverageScopeQueryHandler : IRequestHandler<GetCoverageScopeQuery, ApiResponseDTO<List<GetMiscMasterDto>>>
    {
        private readonly IAssetAmcQueryRepository _iAssetAmcQueryRepository;
        private readonly IMapper _mapper;

        public GetCoverageScopeQueryHandler(IAssetAmcQueryRepository iAssetAmcQueryRepository, IMapper mapper)
        {
            _iAssetAmcQueryRepository = iAssetAmcQueryRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<List<GetMiscMasterDto>>> Handle(GetCoverageScopeQuery request, CancellationToken cancellationToken)
        {
             var coverageTypes = await _iAssetAmcQueryRepository.GetCoverageScopeAsync();
             var coverageTypeDtoList = _mapper.Map<List<GetMiscMasterDto>>(coverageTypes);

            return new ApiResponseDTO<List<GetMiscMasterDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = coverageTypeDtoList,
                TotalCount = coverageTypeDtoList.Count
            };
        }
    }
}