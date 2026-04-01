using AutoMapper;
using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class SalesOrderAmendmentProfile : Profile
    {
        public SalesOrderAmendmentProfile()
        {
            // Amendment Header: entity → DTO
            CreateMap<SalesOrderAmendmentHeader, SalesOrderAmendmentHeaderDto>()
                .ForMember(dest => dest.SalesOrderNo,
                    opt => opt.MapFrom(src => src.SalesOrderHeader != null ? src.SalesOrderHeader.SalesOrderNo : null))
                .ForMember(dest => dest.StatusName,
                    opt => opt.MapFrom(src => src.StatusMisc != null ? src.StatusMisc.Description : null))
                .ForMember(dest => dest.SalesOrderAmendmentDetails,
                    opt => opt.MapFrom(src => src.SalesOrderAmendmentDetails));

            // Amendment Detail: entity → DTO
            CreateMap<SalesOrderAmendmentDetail, SalesOrderAmendmentDetailDto>()
                .ForMember(dest => dest.OldItemName, opt => opt.Ignore());
        }
    }
}
