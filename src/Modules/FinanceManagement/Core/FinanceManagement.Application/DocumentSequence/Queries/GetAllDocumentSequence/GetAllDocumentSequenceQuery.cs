using Contracts.Common;
using FinanceManagement.Application.DocumentSequence.Dto;
using MediatR;

namespace FinanceManagement.Application.DocumentSequence.Queries.GetAllDocumentSequence
{
    public class GetAllDocumentSequenceQuery : IRequest<ApiResponseDTO<List<DocumentSequenceDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
