using AutoMapper;
using SalesManagement.Application.AgentCommissionConfig.Commands.CreateAgentCommissionConfig;
using SalesManagement.Application.AgentCommissionConfig.Commands.UpdateAgentCommissionConfig;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class AgentCommissionConfigProfile : Profile
    {
        public AgentCommissionConfigProfile()
        {
            CreateMap<CreateAgentCommissionConfigCommand, Domain.Entities.AgentCommissionConfig>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateAgentCommissionConfigCommand, Domain.Entities.AgentCommissionConfig>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
