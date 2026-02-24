#nullable disable
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscTypeMaster;
using SalesManagement.Application.MiscTypeMaster.Dto;

namespace SalesManagement.Application.MiscTypeMaster.Queries.GetAllMiscTypeMaster
{
    public class GetAllMiscTypeMasterQueryHandler : IRequestHandler<GetAllMiscTypeMasterQuery, ApiResponseDTO<List<MiscTypeMasterDto>>>
    {
        private readonly IMiscTypeMasterQueryRepository _queryRepository;

        public GetAllMiscTypeMasterQueryHandler(IMiscTypeMasterQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<ApiResponseDTO<List<MiscTypeMasterDto>>> Handle(GetAllMiscTypeMasterQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            return new ApiResponseDTO<List<MiscTypeMasterDto>>
            {
                IsSuccess = true,
                Message = "Misc Type Master list retrieved successfully.",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
