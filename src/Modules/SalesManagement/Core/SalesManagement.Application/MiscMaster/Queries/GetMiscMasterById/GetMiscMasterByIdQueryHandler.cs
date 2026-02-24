#nullable disable
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.MiscMaster.Dto;

namespace SalesManagement.Application.MiscMaster.Queries.GetMiscMasterById
{
    public class GetMiscMasterByIdQueryHandler : IRequestHandler<GetMiscMasterByIdQuery, ApiResponseDTO<MiscMasterDto>>
    {
        private readonly IMiscMasterQueryRepository _queryRepository;

        public GetMiscMasterByIdQueryHandler(IMiscMasterQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<ApiResponseDTO<MiscMasterDto>> Handle(GetMiscMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var dto = await _queryRepository.GetByIdAsync(request.Id);

            if (dto == null)
                throw new EntityNotFoundException($"Misc Master with Id {request.Id} not found.");

            return new ApiResponseDTO<MiscMasterDto>
            {
                IsSuccess = true,
                Message = "Misc Master retrieved successfully.",
                Data = dto
            };
        }
    }
}
