using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using InventoryManagement.Application.Issue.Command.CreateIssueEntry;
using InventoryManagement.Domain.Entities.Issue;
using static InventoryManagement.Application.Issue.Command.CreateIssueEntry.CreateIssueEntryDto;

namespace InventoryManagement.Application.Common.Mappings.Issue
{
    public class IssueEntryProfile :  Profile
    {
        public IssueEntryProfile()
        {
           // ✅ Header mapping
        CreateMap<CreateIssueEntryDto, IssueHeader>()
            .ForMember(dest => dest.IssueNo, opt => opt.Ignore())           // Auto-generate later
            .ForMember(dest => dest.Id, opt => opt.Ignore())                // Auto-increment identity
            .ForMember(dest => dest.IssueHeaderName, opt => opt.MapFrom(src => src.IssueDetails)); // Map child list properly

        // ✅ Detail mapping
        CreateMap<CreateIssueDetailDto, IssueDetail>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())                // DB-generated identity
            .ForMember(dest => dest.IssueHeaderId, opt => opt.Ignore())     // Set later in repository
            .ForMember(dest => dest.MrsIssueDetails, opt => opt.Ignore());  
        }
    }
}