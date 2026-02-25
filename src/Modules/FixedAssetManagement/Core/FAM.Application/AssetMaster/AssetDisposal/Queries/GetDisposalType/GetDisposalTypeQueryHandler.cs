using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetDisposal;
using FAM.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace FAM.Application.AssetMaster.AssetDisposal.Queries.GetDisposalType
{
    public class GetDisposalTypeQueryHandler : IRequestHandler<GetDisposalTypeQuery, ApiResponseDTO<List<GetMiscMasterDto>>>
    {
        private readonly IAssetDisposalQueryRepository _iAssetDisposalQueryRepository;
        private readonly IMapper _mapper;

        public GetDisposalTypeQueryHandler(IAssetDisposalQueryRepository iAssetDisposalQueryRepository, IMapper mapper)
        {
            _iAssetDisposalQueryRepository = iAssetDisposalQueryRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<List<GetMiscMasterDto>>> Handle(GetDisposalTypeQuery request, CancellationToken cancellationToken)
        {
             var disposalTypes = await _iAssetDisposalQueryRepository.GetDisposalType();
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