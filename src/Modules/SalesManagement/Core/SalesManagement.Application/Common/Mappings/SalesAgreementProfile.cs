using AutoMapper;
using SalesManagement.Application.SalesAgreement.Commands.CreateSalesAgreement;
using SalesManagement.Application.SalesAgreement.Dto;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class SalesAgreementProfile : Profile
    {
        public SalesAgreementProfile()
        {
            // Create: Command → Header entity (with nested details).
            // AgreementNo and StatusId are NOT mapped here — the handler sets them post-map.
            CreateMap<CreateSalesAgreementCommand, SalesAgreementHeader>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AgreementNo, opt => opt.Ignore())
                .ForMember(dest => dest.StatusId, opt => opt.Ignore())
                .ForMember(dest => dest.SalesAgreementDetails, opt => opt.MapFrom(src => src.SalesAgreementDetails))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(_ => IsDelete.NotDeleted));

            // Create: detail DTO → detail entity. ReleasedQty defaults to 0 (system-maintained).
            CreateMap<CreateSalesAgreementDetailDto, SalesAgreementDetail>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.SalesAgreementHeaderId, opt => opt.Ignore())
                .ForMember(dest => dest.SalesAgreementHeader, opt => opt.Ignore())
                .ForMember(dest => dest.ReleasedQty, opt => opt.MapFrom(_ => 0m));
        }
    }
}
