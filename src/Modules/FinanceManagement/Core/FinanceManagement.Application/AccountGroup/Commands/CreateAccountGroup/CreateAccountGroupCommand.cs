using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.AccountGroup.Commands.CreateAccountGroup
{
    public class CreateAccountGroupCommand : IRequest<ApiResponseDTO<int>>
    {
        // CompanyId is taken from the session token (IIPAddressService.GetCompanyId()) in the handler — never sent by the client.
        public string GroupCode { get; set; } = null!;
        public string GroupName { get; set; } = null!;

        // Required for Level 1 groups (the statutory head from AccountTypeMaster); must be null below L1.
        public int? AccountTypeId { get; set; }

        // NULL = create a Level 1 (root) group.
        public int? ParentAccountGroupId { get; set; }

        public int SortOrder { get; set; }
    }
}
