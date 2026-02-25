using Contracts.Common;
using MediatR;

namespace UserManagement.Application.Language.Queries.GetLanguages
{
    public class GetLanguageQuery : IRequest<ApiResponseDTO<List<LanguageDTO>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}