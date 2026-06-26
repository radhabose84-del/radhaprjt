using Contracts.Common;
using FinanceManagement.Application.CoaReport.Dto;
using MediatR;

namespace FinanceManagement.Application.CoaReport.Queries.GetFsMappingValidation
{
    // US-GL02-15 (AC4) — pre-go-live FS-mapping (Schedule III) validation report.
    public class GetFsMappingValidationQuery : IRequest<ApiResponseDTO<FsMappingValidationDto>>
    {
    }
}
