using AutoMapper;
using Contracts.Dtos.Lookups.Purchase;
using PurchaseManagement.Application.VendorEvaluationCriteria.Commands.CreateVendorEvaluationCriteria;
using PurchaseManagement.Application.VendorEvaluationCriteria.Commands.UpdateVendorEvaluationCriteria;
using PurchaseManagement.Application.VendorEvaluationCriteria.Dto;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Application.Common.Mappings
{
    public class VendorEvaluationCriteriaProfile : Profile
    {
        public VendorEvaluationCriteriaProfile()
        {
            // Entity → Full DTO
            CreateMap<Domain.Entities.VendorEvaluation.VendorEvaluationCriteria, VendorEvaluationCriteriaDto>();

            // Entity → Lookup DTO (autocomplete)
            CreateMap<Domain.Entities.VendorEvaluation.VendorEvaluationCriteria, VendorEvaluationCriteriaLookupDto>();

            // Create Command → Entity
            CreateMap<CreateVendorEvaluationCriteriaCommand, Domain.Entities.VendorEvaluation.VendorEvaluationCriteria>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            // Update Command → Entity
            CreateMap<UpdateVendorEvaluationCriteriaCommand, Domain.Entities.VendorEvaluation.VendorEvaluationCriteria>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
