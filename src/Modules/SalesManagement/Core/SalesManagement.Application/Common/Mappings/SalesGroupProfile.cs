using AutoMapper;
using SalesManagement.Application.SalesGroup.Commands.CreateSalesGroup;
using SalesManagement.Application.SalesGroup.Commands.UpdateSalesGroup;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class SalesGroupProfile : Profile
    {
        public SalesGroupProfile()
        {
            CreateMap<CreateSalesGroupCommand, Domain.Entities.SalesGroup>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateSalesGroupCommand, Domain.Entities.SalesGroup>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
