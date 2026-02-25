using PurchaseManagement.Application.GRN.GRNEntry.Commands.CreateGRNEntry;
using MediatR;

namespace PurchaseManagement.Application.GRN.GRNEntry.Commands
{
    public class CreateGRNEntryCommand : IRequest<int>
    {
        public CreateGRNEntryDto GrnEntryCreate { get; set; } = null!;
    }
}