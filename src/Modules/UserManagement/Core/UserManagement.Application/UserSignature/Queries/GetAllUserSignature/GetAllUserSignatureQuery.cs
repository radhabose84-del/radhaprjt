using Contracts.Common;
using MediatR;

namespace UserManagement.Application.UserSignature.Queries.GetAllUserSignature
{
    public class GetAllUserSignatureQuery : IRequest<ApiResponseDTO<List<GetAllUserSignatureDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}
