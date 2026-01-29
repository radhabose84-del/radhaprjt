using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using FAM.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace FAM.Application.DepreciationGroup.Queries.GetAssetTypeQuery
{
    public class GetAssetTypeQueryHandler : IRequestHandler<GetAssetTypeQuery, ApiResponseDTO<List<GetMiscMasterDto>>>
    {
        private readonly IAssetMasterGeneralQueryRepository _repository;
        private readonly IMapper _mapper;

        public GetAssetTypeQueryHandler(IAssetMasterGeneralQueryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<List<GetMiscMasterDto>>> Handle(GetAssetTypeQuery request, CancellationToken cancellationToken)
        {
            var DepMethod = await _repository.GetAssetTypeAsync();
            var DepMethodDtoList = _mapper.Map<List<GetMiscMasterDto>>(DepMethod);

            return new ApiResponseDTO<List<GetMiscMasterDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = DepMethodDtoList,
                TotalCount = DepMethodDtoList.Count
            };
        }
    }
}