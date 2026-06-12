using UserManagement.Application.IconMaster.Queries.GetIconMaster;
using MediatR;

namespace UserManagement.Application.IconMaster.Queries.GetIconMasterById
{
    public class GetIconMasterByIdQuery : IRequest<IconMasterDto>
    {
        public int IconMasterId { get; set; }
    }
}
