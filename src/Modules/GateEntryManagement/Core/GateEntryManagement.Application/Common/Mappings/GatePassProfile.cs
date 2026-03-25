using AutoMapper;
using GateEntryManagement.Application.GatePass.Commands.CreateGatePass;
using GateEntryManagement.Application.GatePass.Dto;
using GateEntryManagement.Domain.Entities;
using static GateEntryManagement.Domain.Common.BaseEntity;

namespace GateEntryManagement.Application.Common.Mappings
{
    public class GatePassProfile : Profile
    {
        public GatePassProfile()
        {
            CreateMap<CreateGatePassCommand, GatePassHdr>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.GatePassDetails, opt => opt.Ignore());

            CreateMap<CreateGatePassDetailDto, GatePassDtl>();
        }
    }
}
