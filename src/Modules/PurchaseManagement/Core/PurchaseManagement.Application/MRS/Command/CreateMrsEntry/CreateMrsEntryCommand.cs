using MediatR;

namespace PurchaseManagement.Application.MRS.Command.CreateMrsEntry
{
    public class CreateMrsEntryCommand : IRequest<int>
    {
        public CreateMrsEntryDto MrsEntry { get; set; } = null!;
        
    }
}