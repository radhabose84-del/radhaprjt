using AutoMapper;
using Contracts.Dtos.Lookups.Purchase;
using PurchaseManagement.Application.DeliveryScoreRule.Commands.CreateDeliveryScoreRule;
using PurchaseManagement.Application.DeliveryScoreRule.Commands.UpdateDeliveryScoreRule;
using PurchaseManagement.Application.DeliveryScoreRule.Dto;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Application.Common.Mappings
{
    public class DeliveryScoreRuleProfile : Profile
    {
        public DeliveryScoreRuleProfile()
        {
            // Entity → Full DTO
            CreateMap<Domain.Entities.VendorEvaluation.DeliveryScoreRule, DeliveryScoreRuleDto>();

            // Entity → Lookup DTO (autocomplete)
            CreateMap<Domain.Entities.VendorEvaluation.DeliveryScoreRule, DeliveryScoreRuleLookupDto>();

            // Create Command → Entity
            CreateMap<CreateDeliveryScoreRuleCommand, Domain.Entities.VendorEvaluation.DeliveryScoreRule>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            // Update Command → Entity
            CreateMap<UpdateDeliveryScoreRuleCommand, Domain.Entities.VendorEvaluation.DeliveryScoreRule>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
