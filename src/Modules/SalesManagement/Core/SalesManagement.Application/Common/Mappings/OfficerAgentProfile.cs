using AutoMapper;
using SalesManagement.Application.OfficerAgent.Commands.CreateOfficerAgent;
using SalesManagement.Application.OfficerAgent.Commands.UpdateOfficerAgent;

namespace SalesManagement.Application.Common.Mappings
{
    public class OfficerAgentProfile : Profile
    {
        public OfficerAgentProfile()
        {
            // OfficerAgent does not extend BaseEntity — IsActive is a plain bool
            CreateMap<CreateOfficerAgentCommand, Domain.Entities.OfficerAgent>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            CreateMap<UpdateOfficerAgentCommand, Domain.Entities.OfficerAgent>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1));
        }
    }
}
