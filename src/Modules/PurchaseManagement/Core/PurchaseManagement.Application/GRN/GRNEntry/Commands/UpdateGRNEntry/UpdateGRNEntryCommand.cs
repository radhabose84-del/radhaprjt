using MediatR;

namespace PurchaseManagement.Application.GRN.GRNEntry.Commands.UpdateGRNEntry
{
    public class UpdateGRNEntryCommand : IRequest<bool>
    {
        public UpdateGRNEntryDto GrnEntryUpdate { get; set; } = null!;
    }
}