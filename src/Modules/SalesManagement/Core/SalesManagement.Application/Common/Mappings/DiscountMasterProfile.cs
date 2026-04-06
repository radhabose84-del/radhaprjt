using AutoMapper;
using SalesManagement.Application.DiscountMaster.Commands.CreateDiscountMaster;
using SalesManagement.Application.DiscountMaster.Commands.UpdateDiscountMaster;
using SalesManagement.Application.DiscountMaster.Dto;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class DiscountMasterProfile : Profile
    {
        public DiscountMasterProfile()
        {
            // Entity to DTO
            CreateMap<Domain.Entities.DiscountMaster, DiscountMasterDto>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted == IsDelete.Deleted));

            CreateMap<Domain.Entities.DiscountMaster, DiscountMasterLookupDto>();

            CreateMap<Domain.Entities.DiscountSlab, DiscountSlabDto>();

            // Command to Entity
            CreateMap<CreateDiscountMasterCommand, Domain.Entities.DiscountMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.DiscountSlabs, opt => opt.Ignore())
                .ForMember(dest => dest.DiscountSalesGroups, opt => opt.Ignore())
                .ForMember(dest => dest.DiscountPaymentTerms, opt => opt.Ignore());

            CreateMap<UpdateDiscountMasterCommand, Domain.Entities.DiscountMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(dest => dest.DiscountSlabs, opt => opt.Ignore())
                .ForMember(dest => dest.DiscountSalesGroups, opt => opt.Ignore())
                .ForMember(dest => dest.DiscountPaymentTerms, opt => opt.Ignore());
        }
    }
}
