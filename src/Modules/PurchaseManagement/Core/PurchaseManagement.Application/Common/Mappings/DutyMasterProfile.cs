using AutoMapper;
using PurchaseManagement.Application.DutyMaster;
using PurchaseManagement.Application.PurchaseIndents.Queries.ApprovedIndentDetailsForPO;

namespace PurchaseManagement.Application.Purchase.DutyMaster.Mapping
{
    public class DutyMasterProfile : Profile
    {
        public DutyMasterProfile()
        {
            CreateMap<PurchaseManagement.Domain.Entities.DutyMaster, DutyMasterDto>().ReverseMap();
            CreateMap<PurchaseManagement.Domain.Entities.DutyMaster, DutyMasterDto>();
            CreateMap<PurchaseManagement.Domain.Entities.DutyMaster, DutyMasterViewDto>()
                .ForMember(d => d.Hsn, opt => opt.Ignore())
                .ForMember(d => d.DutyCategoryName, opt => opt.Ignore())
                .ForMember(d => d.CountryOfOriginApplicabilityName, opt => opt.Ignore());
            CreateMap<DutyMasterReadDto, IndentDutyForPODto>()
            .ForMember(d => d.DutyCategoryName, _ => _.Ignore())
            .ForMember(d => d.CountryOfOriginApplicabilityName, _ => _.Ignore());
        }
    }
}
