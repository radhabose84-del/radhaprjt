using AutoMapper;
using SalesManagement.Application.SalesOrderTypeMaster.Commands.CreateSalesOrderTypeMaster;
using SalesManagement.Application.SalesOrderTypeMaster.Commands.UpdateSalesOrderTypeMaster;
using SalesManagement.Application.SalesOrderTypeMaster.Dto;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class SalesOrderTypeMasterProfile : Profile
    {
        public SalesOrderTypeMasterProfile()
        {
            // Entity to DTO
            CreateMap<Domain.Entities.SalesOrderTypeMaster, SalesOrderTypeMasterDto>()
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => src.IsActive  == Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted == IsDelete.Deleted))
                // The following are populated by the query repository / handler — ignore on auto-map
                .ForMember(dest => dest.SoTypeCode,         opt => opt.Ignore())
                .ForMember(dest => dest.SoTypeName,         opt => opt.Ignore())
                .ForMember(dest => dest.TaxTypeName,        opt => opt.Ignore())
                .ForMember(dest => dest.TaxTypeShortName,   opt => opt.Ignore())
                .ForMember(dest => dest.DefaultCurrencyCode,opt => opt.Ignore());

            CreateMap<Domain.Entities.SalesOrderTypeMaster, SalesOrderTypeMasterLookupDto>()
                .ForMember(dest => dest.SoTypeCode,       opt => opt.Ignore())
                .ForMember(dest => dest.TaxTypeShortName, opt => opt.Ignore());

            // Command to Entity
            CreateMap<CreateSalesOrderTypeMasterCommand, Domain.Entities.SalesOrderTypeMaster>()
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(_ => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(_ => IsDelete.NotDeleted));

            CreateMap<UpdateSalesOrderTypeMasterCommand, Domain.Entities.SalesOrderTypeMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive))
                // SoTypeId + TaxTypeId are immutable — not present in Update command, so ignore
                .ForMember(dest => dest.SoTypeId,  opt => opt.Ignore())
                .ForMember(dest => dest.TaxTypeId, opt => opt.Ignore());
        }
    }
}
