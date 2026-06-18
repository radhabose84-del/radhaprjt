using Contracts.Common;
using FinanceManagement.Application.GlAccountImport.Dto;
using MediatR;

namespace FinanceManagement.Application.GlAccountImport.Queries.GetGlAccountImportLogs
{
    public class GetGlAccountImportLogsQuery : IRequest<ApiResponseDTO<List<GlAccountImportLogDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
