using AutoMapper;
using SalesManagement.Application.AgentCommissionConfig.Commands.CreateAgentCommissionConfig;
using SalesManagement.Application.AgentCommissionConfig.Commands.UpdateAgentCommissionConfig;
using SalesManagement.Application.AgentCommissionConfig.Dto;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class AgentCommissionConfigProfile : Profile
    {
        public AgentCommissionConfigProfile()
        {
            // Command → Entity mappings
            CreateMap<CreateAgentCommissionConfigCommand, Domain.Entities.AgentCommissionConfig>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.AgentCommissionSalesGroups, opt => opt.Ignore())
                .ForMember(dest => dest.AgentCommissionPaymentTerms, opt => opt.Ignore())
                .ForMember(dest => dest.AgentCommissionSlabs, opt => opt.Ignore());

            CreateMap<UpdateAgentCommissionConfigCommand, Domain.Entities.AgentCommissionConfig>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(dest => dest.AgentCommissionSalesGroups, opt => opt.Ignore())
                .ForMember(dest => dest.AgentCommissionPaymentTerms, opt => opt.Ignore())
                .ForMember(dest => dest.AgentCommissionSlabs, opt => opt.Ignore());

            // Entity → DTO mappings
            CreateMap<Domain.Entities.AgentCommissionConfig, AgentCommissionConfigDto>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted == IsDelete.Deleted))
                .ForMember(dest => dest.SalesGroups, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentTerms, opt => opt.Ignore())
                .ForMember(dest => dest.Slabs, opt => opt.Ignore());
        }
    }
}
