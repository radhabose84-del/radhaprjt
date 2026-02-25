using AutoMapper;
using SalesManagement.Application.SalesGroup.Commands.CreateSalesGroup;
using SalesManagement.Application.SalesGroup.Commands.UpdateSalesGroup;

namespace SalesManagement.Application.Common.Mappings
{
    public class SalesGroupProfile : Profile
    {
        public SalesGroupProfile()
        {
            CreateMap<CreateSalesGroupCommand, Domain.Entities.SalesGroup>();
            CreateMap<UpdateSalesGroupCommand, Domain.Entities.SalesGroup>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1
                        ? Domain.Common.BaseEntity.Status.Active
                        : Domain.Common.BaseEntity.Status.Inactive));
        }
    }
}
