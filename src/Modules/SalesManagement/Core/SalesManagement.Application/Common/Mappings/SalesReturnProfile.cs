using AutoMapper;
using SalesManagement.Application.SalesReturn.Commands.CreateSalesReturn;
using SalesManagement.Application.SalesReturn.Dto;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class SalesReturnProfile : Profile
    {
        public SalesReturnProfile()
        {
            CreateMap<CreateSalesReturnCommand, SalesReturnHeader>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.SalesReturnDetails, opt => opt.Ignore());

            CreateMap<CreateSalesReturnItemDto, SalesReturnDetail>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.InvoiceHeaderId, opt => opt.Ignore());
        }
    }
}
