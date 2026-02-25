using AutoMapper;
using SalesManagement.Application.SalesChannel.Commands.CreateSalesChannel;
using SalesManagement.Application.SalesChannel.Commands.UpdateSalesChannel;

namespace SalesManagement.Application.Common.Mappings
{
    public class SalesChannelProfile : Profile
    {
        public SalesChannelProfile()
        {
            CreateMap<CreateSalesChannelCommand, Domain.Entities.SalesChannel>();
            CreateMap<UpdateSalesChannelCommand, Domain.Entities.SalesChannel>();
        }
    }
}
