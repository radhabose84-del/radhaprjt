using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetAmc;
using FAM.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace FAM.Application.AssetMaster.AssetAmc.Queries.GetRenewStatus
{
    public class GetRenewStatusQueryHandler : IRequestHandler<GetRenewStatusQuery, ApiResponseDTO<List<GetMiscMasterDto>>>
    {
        private readonly IAssetAmcQueryRepository _iAssetAmcQueryRepository;
        private readonly IMapper _mapper;

        public GetRenewStatusQueryHandler(IAssetAmcQueryRepository iAssetAmcQueryRepository, IMapper mapper)
        {
            _iAssetAmcQueryRepository = iAssetAmcQueryRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<List<GetMiscMasterDto>>> Handle(GetRenewStatusQuery request, CancellationToken cancellationToken)
        {
            var renewTypes = await _iAssetAmcQueryRepository.GetRenewStatusAsync();
            var renewTypeDtoList = _mapper.Map<List<GetMiscMasterDto>>(renewTypes);

            return new ApiResponseDTO<List<GetMiscMasterDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = renewTypeDtoList,
                TotalCount = renewTypeDtoList.Count
            };
        }
    }
}