using AutoMapper;
using SalesManagement.Application.CustomerVisit.Commands.CreateCustomerVisit;
using SalesManagement.Application.CustomerVisit.Commands.UpdateCustomerVisit;
using SalesManagement.Application.CustomerVisit.Dto;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class CustomerVisitProfile : Profile
    {
        public CustomerVisitProfile()
        {
            // Entity to DTO
            CreateMap<Domain.Entities.CustomerVisit, CustomerVisitDto>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted == IsDelete.Deleted))
                .ForMember(dest => dest.Products, opt => opt.Ignore());

            CreateMap<Domain.Entities.CustomerVisit, CustomerVisitLookupDto>();

            // Create Command → Entity
            CreateMap<CreateCustomerVisitCommand, Domain.Entities.CustomerVisit>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerVisitProducts, opt => opt.MapFrom(src => src.Products))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            // Update Command → Entity
            CreateMap<UpdateCustomerVisitCommand, Domain.Entities.CustomerVisit>()
                .ForMember(dest => dest.CustomerVisitProducts, opt => opt.MapFrom(src => src.Products))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));

            // Detail DTO → Detail Entity
            CreateMap<CreateCustomerVisitProductDto, Domain.Entities.CustomerVisitProduct>();
        }
    }
}
