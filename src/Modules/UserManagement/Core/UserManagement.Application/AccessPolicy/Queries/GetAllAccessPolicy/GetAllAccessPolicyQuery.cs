using Contracts.Common;
using MediatR;
using UserManagement.Application.AccessPolicy.Dto;

namespace UserManagement.Application.AccessPolicy.Queries.GetAllAccessPolicy
{
    public class GetAllAccessPolicyQuery : IRequest<ApiResponseDTO<List<AccessPolicyDto>>>
    {
        public int     PageNumber  { get; set; }
        public int     PageSize    { get; set; }
        public string? SearchTerm  { get; set; }
    }
}
