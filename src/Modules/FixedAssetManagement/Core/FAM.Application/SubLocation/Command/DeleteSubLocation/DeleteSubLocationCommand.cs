using MediatR;

namespace FAM.Application.Location.Command.DeleteAubLocation
{
    public class DeleteSubLocationCommand : IRequest<bool>
    {
        public int Id { get; set; }
        
    }
}