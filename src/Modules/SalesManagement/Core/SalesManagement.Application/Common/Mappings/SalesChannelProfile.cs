#nullable disable
using AutoMapper;
using SalesManagement.Application.SalesChannel.Commands.CreateSalesChannel;
using SalesManagement.Application.SalesChannel.Commands.UpdateSalesChannel;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class SalesChannelProfile : Profile
    {
        public SalesChannelProfile()
        {
            CreateMap<CreateSalesChannelCommand, Domain.Entities.SalesChannel>()
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateSalesChannelCommand, Domain.Entities.SalesChannel>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
