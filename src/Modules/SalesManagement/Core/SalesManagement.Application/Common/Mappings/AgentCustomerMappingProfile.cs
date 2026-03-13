using AutoMapper;
using SalesManagement.Application.AgentCustomerMapping.Commands.CreateAgentCustomerMapping;
using SalesManagement.Application.AgentCustomerMapping.Commands.UpdateAgentCustomerMapping;
using SalesManagement.Application.AgentCustomerMapping.Dto;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class AgentCustomerMappingProfile : Profile
    {
        public AgentCustomerMappingProfile()
        {
            CreateMap<CreateAgentCustomerMappingCommand, Domain.Entities.AgentCustomerMapping>()
                .ForMember(dest => dest.IsActive,   opt => opt.MapFrom(_ => Status.Active))
                .ForMember(dest => dest.IsDeleted,  opt => opt.MapFrom(_ => IsDelete.NotDeleted));

            CreateMap<UpdateAgentCustomerMappingCommand, Domain.Entities.AgentCustomerMapping>()
                .ForMember(dest => dest.IsActive,   opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<Domain.Entities.AgentCustomerMapping, AgentCustomerMappingDto>();
        }
    }
}
