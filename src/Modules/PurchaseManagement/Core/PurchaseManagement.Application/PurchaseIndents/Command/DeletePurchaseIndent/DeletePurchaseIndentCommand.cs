using MediatR;

namespace PurchaseManagement.Application.PurchaseIndents.Command.DeletePurchaseIndent
{
    public class DeletePurchaseIndentCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}