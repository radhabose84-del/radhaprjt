using AutoMapper;
using GateEntryManagement.Application.GateInward.Commands.CreateGateInward;
using GateEntryManagement.Application.GateInward.Dto;
using GateEntryManagement.Domain.Entities;
using static GateEntryManagement.Domain.Common.BaseEntity;

namespace GateEntryManagement.Application.Common.Mappings
{
    public class GateInwardProfile : Profile
    {
        public GateInwardProfile()
        {
            CreateMap<CreateGateInwardCommand, GateInwardHdr>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.GateInwardDetails, opt => opt.Ignore())
                // Attachment is staged then set explicitly in the handler (not name-mapped)
                .ForMember(dest => dest.AttachmentFileName, opt => opt.Ignore())
                .ForMember(dest => dest.AttachmentFilePath, opt => opt.Ignore());

            CreateMap<CreateGateInwardDetailDto, GateInwardDtl>();
        }
    }
}
