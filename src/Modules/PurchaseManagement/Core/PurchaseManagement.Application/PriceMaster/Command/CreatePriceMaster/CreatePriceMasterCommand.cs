using MediatR;
using PurchaseManagement.Application.PriceMaster.Dtos;

namespace PurchaseManagement.Application.PriceMaster.Commands.Create
{
    public sealed class CreatePriceMasterCommand : IRequest<int>
    {
        public PriceMasterCreateDto Data { get; init; } = default!;
    }
}
