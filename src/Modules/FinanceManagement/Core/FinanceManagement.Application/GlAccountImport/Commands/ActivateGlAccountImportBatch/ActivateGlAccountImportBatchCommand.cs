using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.GlAccountImport.Commands.ActivateGlAccountImportBatch
{
    public sealed record ActivateGlAccountImportBatchCommand(int ImportLogId)
        : IRequest<ApiResponseDTO<int>>;
}
