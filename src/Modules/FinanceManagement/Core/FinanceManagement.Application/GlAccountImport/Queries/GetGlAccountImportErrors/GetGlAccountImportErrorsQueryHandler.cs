using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IGlAccountImport;
using FinanceManagement.Application.GlAccountImport.Dto;
using MediatR;

namespace FinanceManagement.Application.GlAccountImport.Queries.GetGlAccountImportErrors
{
    public class GetGlAccountImportErrorsQueryHandler
        : IRequestHandler<GetGlAccountImportErrorsQuery, ApiResponseDTO<List<GlAccountImportErrorDto>>>
    {
        private readonly IGlAccountImportQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;

        public GetGlAccountImportErrorsQueryHandler(
            IGlAccountImportQueryRepository queryRepository,
            IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<List<GlAccountImportErrorDto>>> Handle(
            GetGlAccountImportErrorsQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            if (request.ImportLogId <= 0 ||
                !await _queryRepository.LogBelongsToCompanyAsync(request.ImportLogId, companyId))
            {
                return new ApiResponseDTO<List<GlAccountImportErrorDto>>
                {
                    IsSuccess = false,
                    Message = "Import batch not found.",
                    Data = new List<GlAccountImportErrorDto>()
                };
            }

            var errors = (await _queryRepository.GetErrorsAsync(request.ImportLogId)).ToList();

            return new ApiResponseDTO<List<GlAccountImportErrorDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = errors,
                TotalCount = errors.Count
            };
        }
    }
}
