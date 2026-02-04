// Core.Application/Common/Mappings/Item/PutAwayProfile.cs
using AutoMapper;
using InventoryManagement.Application.Item.PutAway.Commands.CreatePutAwayRule;
using InventoryManagement.Application.Item.PutAway.Queries.GetAllPutAwayRule;
using InventoryManagement.Domain.Entities.Item.PutAway;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Application.Common.Mappings.Item
{
    public sealed class PutAwayProfile : Profile
    {
        public PutAwayProfile()
        {
            // Entity -> DTO (1/0)
            CreateMap<PutAwayRule, PutAwayRuleDetailDto>()
                .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive == Status.Active ? 1 : 0));

            CreateMap<PutAwayStrategy, PutAwayStrategyDto>()
                .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive == Status.Active ? 1 : 0));

            CreateMap<PutAwayRule, PutAwayRuleListDto>()
                .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive == Status.Active ? 1 : 0));

            // Request (byte 1/0) -> Entity (enum)
            CreateMap<CreatePutAwayRuleRequest, PutAwayRule>()
                .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<CreatePutAwayStrategyRequest, PutAwayStrategy>()
                .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
