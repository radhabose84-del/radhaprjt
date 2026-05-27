using AutoMapper;
using Contracts.Dtos.Lookups.Purchase;
using PurchaseManagement.Application.VendorRatingGrade.Commands.CreateVendorRatingGrade;
using PurchaseManagement.Application.VendorRatingGrade.Commands.UpdateVendorRatingGrade;
using PurchaseManagement.Application.VendorRatingGrade.Dto;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Application.Common.Mappings
{
    public class VendorRatingGradeProfile : Profile
    {
        public VendorRatingGradeProfile()
        {
            // Entity → Full DTO
            CreateMap<Domain.Entities.VendorEvaluation.VendorRatingGrade, VendorRatingGradeDto>();

            // Entity → Lookup DTO (autocomplete)
            CreateMap<Domain.Entities.VendorEvaluation.VendorRatingGrade, VendorRatingGradeLookupDto>();

            // Create Command → Entity
            CreateMap<CreateVendorRatingGradeCommand, Domain.Entities.VendorEvaluation.VendorRatingGrade>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            // Update Command → Entity
            CreateMap<UpdateVendorRatingGradeCommand, Domain.Entities.VendorEvaluation.VendorRatingGrade>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
