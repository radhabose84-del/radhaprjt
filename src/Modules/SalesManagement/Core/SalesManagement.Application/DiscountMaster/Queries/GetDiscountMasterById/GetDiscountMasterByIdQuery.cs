using MediatR;
using SalesManagement.Application.DiscountMaster.Dto;

namespace SalesManagement.Application.DiscountMaster.Queries.GetDiscountMasterById
{
    public class GetDiscountMasterByIdQuery : IRequest<DiscountMasterDto?>
    {
        public int Id { get; set; }
    }
}
