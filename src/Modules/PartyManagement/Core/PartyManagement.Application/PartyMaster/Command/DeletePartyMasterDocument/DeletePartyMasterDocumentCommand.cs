using MediatR;
using Contracts.Common;

namespace PartyManagement.Application.PartyMaster.Command.DeletePartyMasterDocument
{
    public class DeletePartyMasterDocumentCommand : IRequest<bool>, IRequirePermission
    {
        //Local Folder Delete
        public string? partydocumentPath { get; set; }

        //DB Delete Check
        public int Id { get; set; }
        public int PartyId { get; set; }
        public string? FileName { get; set; }  
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
