#nullable disable
using AutoMapper;
using SalesManagement.Application.SalesOffice.Commands.CreateSalesOffice;
using SalesManagement.Application.SalesOffice.Commands.UpdateSalesOffice;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class SalesOfficeProfile : Profile
    {
        public SalesOfficeProfile()
        {
            CreateMap<CreateSalesOfficeCommand, Domain.Entities.SalesOffice>()
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateSalesOfficeCommand, Domain.Entities.SalesOffice>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
