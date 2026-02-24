#nullable disable
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscTypeMaster;
using SalesManagement.Application.MiscTypeMaster.Dto;

namespace SalesManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById
{
    public class GetMiscTypeMasterByIdQueryHandler : IRequestHandler<GetMiscTypeMasterByIdQuery, ApiResponseDTO<MiscTypeMasterDto>>
    {
        private readonly IMiscTypeMasterQueryRepository _queryRepository;

        public GetMiscTypeMasterByIdQueryHandler(IMiscTypeMasterQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<ApiResponseDTO<MiscTypeMasterDto>> Handle(GetMiscTypeMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var dto = await _queryRepository.GetByIdAsync(request.Id);

            if (dto == null)
                throw new EntityNotFoundException($"Misc Type Master with Id {request.Id} not found.");

            return new ApiResponseDTO<MiscTypeMasterDto>
            {
                IsSuccess = true,
                Message = "Misc Type Master retrieved successfully.",
                Data = dto
            };
        }
    }
}
