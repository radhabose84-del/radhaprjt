using AutoMapper;
using SalesManagement.Application.SalesContact.Commands.CreateSalesContact;
using SalesManagement.Application.SalesContact.Commands.UpdateSalesContact;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class SalesContactProfile : Profile
    {
        public SalesContactProfile()
        {
            CreateMap<SalesManagement.Domain.Entities.SalesContact,
                      SalesManagement.Application.SalesContact.Dto.SalesContactDto>();

            CreateMap<SalesManagement.Domain.Entities.SalesContact,
                      SalesManagement.Application.SalesContact.Dto.SalesContactLookupDto>();

            CreateMap<CreateSalesContactCommand, SalesManagement.Domain.Entities.SalesContact>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateSalesContactCommand, SalesManagement.Domain.Entities.SalesContact>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
