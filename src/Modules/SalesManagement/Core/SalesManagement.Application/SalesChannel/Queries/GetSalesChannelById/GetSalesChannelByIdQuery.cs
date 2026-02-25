using MediatR;
using SalesManagement.Application.SalesChannel.Dto;

namespace SalesManagement.Application.SalesChannel.Queries.GetSalesChannelById
{
    public class GetSalesChannelByIdQuery : IRequest<SalesChannelDto>
    {
        public int Id { get; set; }
    }
}
