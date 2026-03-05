using AutoMapper;
using SalesManagement.Application.OfficerAgent.Commands.CreateOfficerAgent;
using SalesManagement.Application.OfficerAgent.Commands.UpdateOfficerAgent;

namespace SalesManagement.Application.Common.Mappings
{
    public class OfficerAgentProfile : Profile
    {
        public OfficerAgentProfile()
        {
            // Batch create — maps the whole command to a list of entities
            CreateMap<CreateOfficerAgentCommand, List<Domain.Entities.OfficerAgent>>()
                .ConvertUsing((src, dest, ctx) => src.Agents.Select(item =>
                    new Domain.Entities.OfficerAgent
                    {
                        AgentId = item.AgentId,
                        MarketingOfficerId = src.MarketingOfficerId,
                        ValidityFrom = item.ValidityFrom,
                        ValidityTo = item.ValidityTo,
                        IsActive = item.IsActive == 1
                    }).ToList());

            // Batch update — maps each item, preserving Id and mapping IsActive from int
            CreateMap<UpdateOfficerAgentCommand, List<Domain.Entities.OfficerAgent>>()
                .ConvertUsing((src, dest, ctx) => src.Agents.Select(item =>
                    new Domain.Entities.OfficerAgent
                    {
                        Id = item.Id,
                        AgentId = item.AgentId,
                        MarketingOfficerId = src.MarketingOfficerId,
                        ValidityFrom = item.ValidityFrom,
                        ValidityTo = item.ValidityTo,
                        IsActive = item.IsActive == 1
                    }).ToList());
        }
    }
}
