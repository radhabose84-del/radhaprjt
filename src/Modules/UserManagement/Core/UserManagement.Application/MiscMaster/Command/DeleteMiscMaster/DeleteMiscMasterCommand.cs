using MediatR;

namespace UserManagement.Application.MiscMaster.Command.DeleteMiscMaster
{
    public class DeleteMiscMasterCommand: IRequest<bool>
    {
          public int Id { get; set; }
        
    }
}