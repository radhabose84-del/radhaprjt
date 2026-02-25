using MediatR;
using SalesManagement.Application.SalesItemPriceMaster.Dto;

namespace SalesManagement.Application.SalesItemPriceMaster.Queries.GetSalesItemPriceMasterById
{
    public class GetSalesItemPriceMasterByIdQuery : IRequest<SalesItemPriceMasterDto>
    {
        public int Id { get; set; }
    }
}
