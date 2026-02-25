using AutoMapper;
// using PartyManagement.Application.GRN.GateEntry.Commands.CreateGateEntry;
using PurchaseManagement.Application.GRN.GateEntry.Commands.CreateGateEntry;
using PurchaseManagement.Domain.Entities.GRN.GateEntry;
using static PurchaseManagement.Application.GRN.GateEntry.Commands.CreateGateEntry.CreateGateEntryDto;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Application.Common.Mappings.GRN.GateEntry
{
    public class GateEntryProfile : Profile
    {
        public GateEntryProfile()
        {
            CreateMap<CreateGateEntryDto, GateEntryHeader>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.GateEntryDetails, opt => opt.MapFrom(src => src.GateEntryDetails))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));
             CreateMap<GateEntryDetailDto, GateEntryDetail>();

        }
    }
}