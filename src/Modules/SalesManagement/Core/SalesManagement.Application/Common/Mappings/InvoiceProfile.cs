using AutoMapper;
using SalesManagement.Application.Invoice.Commands.CreateInvoice;
using SalesManagement.Application.Invoice.Commands.UpdateInvoice;
using SalesManagement.Application.Invoice.Dto;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class InvoiceProfile : Profile
    {
        public InvoiceProfile()
        {
            // Command → Entity (for create handler)
            CreateMap<CreateInvoiceCommand, InvoiceHeader>()
                .ForMember(dest => dest.InvoiceNo,      opt => opt.Ignore())    // auto-generated
                .ForMember(dest => dest.InvoiceDetails, opt => opt.MapFrom(src => src.Details))
                .ForMember(dest => dest.IsActive,       opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted,      opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<CreateInvoiceDetailDto, InvoiceDetail>();

            // Command → Entity (for update handler)
            CreateMap<UpdateInvoiceCommand, InvoiceHeader>()
                .ForMember(dest => dest.InvoiceNo,        opt => opt.Ignore())   // immutable
                .ForMember(dest => dest.DispatchAdviceId, opt => opt.Ignore())   // immutable
                .ForMember(dest => dest.PartyId,          opt => opt.Ignore())   // immutable
                .ForMember(dest => dest.UnitId,           opt => opt.Ignore())   // immutable
                .ForMember(dest => dest.FinancialYearId,  opt => opt.Ignore())   // immutable
                .ForMember(dest => dest.IsDeleted,        opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceDetails,   opt => opt.MapFrom(src => src.Details))
                .ForMember(dest => dest.IsActive,         opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<UpdateInvoiceDetailDto, InvoiceDetail>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());   // always re-inserted as new rows

            // Entity → DTO (for queries — Dapper returns DTOs directly; these mappings used if needed)
            CreateMap<InvoiceHeader, InvoiceHeaderDto>()
                .ForMember(dest => dest.IsActive,          opt => opt.MapFrom(src => src.IsActive  == Status.Active))
                .ForMember(dest => dest.IsDeleted,         opt => opt.MapFrom(src => src.IsDeleted == IsDelete.Deleted))
                .ForMember(dest => dest.InvoiceTypeName,   opt => opt.Ignore())
                .ForMember(dest => dest.DispatchNo,        opt => opt.Ignore())
                .ForMember(dest => dest.PartyName,         opt => opt.Ignore())
                .ForMember(dest => dest.AgentName,         opt => opt.Ignore())
                .ForMember(dest => dest.UnitName,          opt => opt.Ignore())
                .ForMember(dest => dest.TransportModeName, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceDetails,    opt => opt.Ignore());

            CreateMap<InvoiceDetail, InvoiceDetailDto>()
                .ForMember(dest => dest.ItemName,     opt => opt.Ignore())
                .ForMember(dest => dest.PackTypeName, opt => opt.Ignore())
                .ForMember(dest => dest.UOMName,      opt => opt.Ignore())    // populated via IUOMLookup
                .ForMember(dest => dest.LotNo,        opt => opt.Ignore());   // populated via JOIN in query repo

            CreateMap<EInvoiceHeader, EInvoiceHeaderDto>()
                .ForMember(dest => dest.IsActive,    opt => opt.MapFrom(src => src.IsActive == Status.Active))
                .ForMember(dest => dest.PartyName,   opt => opt.Ignore())
                .ForMember(dest => dest.StatusName,  opt => opt.Ignore())
                .ForMember(dest => dest.EInvoiceDetails, opt => opt.Ignore());

            CreateMap<EInvoiceDetail, EInvoiceDetailDto>();
        }
    }
}
