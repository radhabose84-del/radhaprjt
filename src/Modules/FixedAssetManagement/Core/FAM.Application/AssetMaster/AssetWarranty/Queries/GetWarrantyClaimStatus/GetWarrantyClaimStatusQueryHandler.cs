using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetWarranty;
using FAM.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace FAM.Application.AssetMaster.AssetWarranty.Queries.GetWarrantyClaimStatus
{
    public class GetWarrantyClaimStatusQueryHandler  : IRequestHandler<GetWarrantyClaimStatusQuery, ApiResponseDTO<List<GetMiscMasterDto>>>
    {
        private readonly IAssetWarrantyQueryRepository _repository;
        private readonly IMapper _mapper;

        public GetWarrantyClaimStatusQueryHandler(IAssetWarrantyQueryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<List<GetMiscMasterDto>>> Handle(GetWarrantyClaimStatusQuery request, CancellationToken cancellationToken)
        {
            var warrantyType = await _repository.GetWarrantyClaimStatusAsync();
            var warrantyTypeList = _mapper.Map<List<GetMiscMasterDto>>(warrantyType);

            return new ApiResponseDTO<List<GetMiscMasterDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = warrantyTypeList,
                TotalCount = warrantyTypeList.Count
            };
        }
    }
}