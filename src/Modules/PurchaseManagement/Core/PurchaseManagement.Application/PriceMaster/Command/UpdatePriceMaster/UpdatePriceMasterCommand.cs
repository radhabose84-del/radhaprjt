using MediatR;
using PurchaseManagement.Application.PriceMaster.Dtos;

namespace PurchaseManagement.Application.PriceMaster.Commands.Update
{
    public sealed class UpdatePriceMasterCommand : IRequest<bool>
    {
        public PriceMasterUpdateDto Data { get; set; } = default!;
    }
}
