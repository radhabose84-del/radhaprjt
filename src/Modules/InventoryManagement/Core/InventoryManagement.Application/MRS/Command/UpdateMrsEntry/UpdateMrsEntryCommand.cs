using MediatR;

namespace InventoryManagement.Application.MRS.Command.UpdateMrsEntry
{
    public class UpdateMrsEntryCommand  : IRequest<bool>
    {
        public UpdateMrsEntryDto updateMrsEntry { get; set; } = null!;
    }
}