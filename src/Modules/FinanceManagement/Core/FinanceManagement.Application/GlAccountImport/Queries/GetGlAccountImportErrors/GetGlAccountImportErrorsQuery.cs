using Contracts.Common;
using FinanceManagement.Application.GlAccountImport.Dto;
using MediatR;

namespace FinanceManagement.Application.GlAccountImport.Queries.GetGlAccountImportErrors
{
    public sealed record GetGlAccountImportErrorsQuery(int ImportLogId)
        : IRequest<ApiResponseDTO<List<GlAccountImportErrorDto>>>;
}
