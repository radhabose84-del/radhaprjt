using MediatR;

namespace FAM.Application.UOM.Command.DeleteUOM
{
    public class DeleteUOMCommand : IRequest<bool>
    {
        public int Id { get; set; }
        
    }
}