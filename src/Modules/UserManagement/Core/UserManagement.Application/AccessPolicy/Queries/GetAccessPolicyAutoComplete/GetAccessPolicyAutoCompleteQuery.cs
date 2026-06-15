using MediatR;
using UserManagement.Application.AccessPolicy.Dto;

namespace UserManagement.Application.AccessPolicy.Queries.GetAccessPolicyAutoComplete
{
    public sealed record GetAccessPolicyAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<AccessPolicyDto>>;
}
