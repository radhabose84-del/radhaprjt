using MediatR;

namespace FAM.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster
{
    public class DeleteMiscTypeMasterCommand  : IRequest<bool>
    {
          public int Id { get; set; }
    }
}