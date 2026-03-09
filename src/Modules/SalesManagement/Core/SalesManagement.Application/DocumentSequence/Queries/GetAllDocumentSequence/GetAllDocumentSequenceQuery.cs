using Contracts.Common;
using MediatR;
using SalesManagement.Application.DocumentSequence.Dto;

namespace SalesManagement.Application.DocumentSequence.Queries.GetAllDocumentSequence
{
    public class GetAllDocumentSequenceQuery : IRequest<ApiResponseDTO<List<DocumentSequenceDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
