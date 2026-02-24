#nullable disable
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.MiscMaster.Dto;

namespace SalesManagement.Application.MiscMaster.Queries.GetAllMiscMaster
{
    public class GetAllMiscMasterQueryHandler : IRequestHandler<GetAllMiscMasterQuery, ApiResponseDTO<List<MiscMasterDto>>>
    {
        private readonly IMiscMasterQueryRepository _queryRepository;

        public GetAllMiscMasterQueryHandler(IMiscMasterQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<ApiResponseDTO<List<MiscMasterDto>>> Handle(GetAllMiscMasterQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm, request.MiscTypeId);

            return new ApiResponseDTO<List<MiscMasterDto>>
            {
                IsSuccess = true,
                Message = "Misc Master list retrieved successfully.",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
