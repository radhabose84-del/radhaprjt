using MediatR;
using PurchaseManagement.Application.PriceMaster.Dtos;

namespace PurchaseManagement.Application.PriceMaster.Queries.GetById
{
    public sealed class GetPriceMasterByIdQuery : IRequest<PriceMasterGetAllDto?>
    {
        public int Id { get; init; }
    }
}
