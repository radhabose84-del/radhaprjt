using MediatR;

namespace PurchaseManagement.Application.MRS.Command.UpdateMrsEntry
{
    public class UpdateMrsEntryCommand : IRequest<bool>
    {
        public UpdateMrsEntryDto updateMrsEntry { get; set; } = null!;
    }
}