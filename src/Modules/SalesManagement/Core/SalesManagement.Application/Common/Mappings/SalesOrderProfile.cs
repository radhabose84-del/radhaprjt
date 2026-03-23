using AutoMapper;
using SalesManagement.Application.SalesOrder.Commands.UpdateSalesOrder;
using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Application.SalesOrder.Queries.GetPendingSalesOrderById;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class SalesOrderProfile : Profile
    {
        public SalesOrderProfile()
        {
            // Create: DTO → Header entity (with nested details)
            CreateMap<CreateSalesOrderDto, SalesOrderHeader>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.SalesOrderNo, opt => opt.Ignore())
                .ForMember(dest => dest.SalesOrderDetails, opt => opt.MapFrom(src => src.SalesOrderDetails))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            // Create: Detail DTO → Detail entity
            CreateMap<CreateSalesOrderDetailDto, SalesOrderDetail>()
                .ForMember(dest => dest.DispatchedQty, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.PendingQty, opt => opt.MapFrom(src => src.QtyInBags));

            // Update: Command → Header entity
            CreateMap<UpdateSalesOrderCommand, SalesOrderHeader>()
                .ForMember(dest => dest.SalesOrderNo, opt => opt.Ignore())
                .ForMember(dest => dest.SalesOrderDetails, opt => opt.MapFrom(src => src.SalesOrderDetails))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));

            // Update: Detail DTO → Detail entity
            CreateMap<UpdateSalesOrderDetailDto, SalesOrderDetail>();

            // Autocomplete: LookupDto → LookupDto (collection type conversion)
            CreateMap<SalesOrderLookupDto, SalesOrderLookupDto>();

            // Pending: SalesOrderHeaderDto → PendingSalesOrderByIdDto
            CreateMap<SalesOrderHeaderDto, PendingSalesOrderByIdDto>();
        }
    }
}
