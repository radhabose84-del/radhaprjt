using Contracts.Common;
using MediatR;

namespace UserManagement.Application.CustomFields.Queries.GetCustomField
{
    public class GetCustomFieldQuery : IRequest<ApiResponseDTO<List<CustomFieldDTO>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}