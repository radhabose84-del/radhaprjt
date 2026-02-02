// Core.Application/PriceMaster/Commands/Delete/SoftDeletePriceMasterCommand.cs
using MediatR;

namespace PurchaseManagement.Application.PriceMaster.Commands.Delete
{
    public sealed class DeletePriceMasterCommand : IRequest<bool>
    {
        public int Id { get; init; }        
    }
}
