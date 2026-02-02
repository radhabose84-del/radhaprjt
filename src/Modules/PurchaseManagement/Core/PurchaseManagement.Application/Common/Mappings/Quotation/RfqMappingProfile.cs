using AutoMapper;
using PurchaseManagement.Application.Quotation.RfqEntry.Commands.Create;
using PurchaseManagement.Application.Quotation.RfqEntry.Dtos;
using PurchaseManagement.Application.Quotation.RfqEntry.DTOs;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.Quotation.RfqEntry;
using PurchaseManagement.Domain.Entities.ValueObjects;
using System;
using System.Linq;
using static PurchaseManagement.Domain.Common.BaseEntity;
using DomainBase = PurchaseManagement.Domain.Common.BaseEntity; 
namespace PurchaseManagement.Application.Common.Mappings.Quotation
{
    public class RfqMappingProfile : Profile
    {
        public RfqMappingProfile()
        {
            // ---------- CREATE: command -> aggregate ----------
            CreateMap<CreateRfqCommand, RfqMaster>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.RfqCode, o => o.Ignore())
                .ForMember(d => d.UnitId, o => o.Ignore())
                .ForMember(d => d.RfqStatusId, o => o.Ignore())
                .ForMember(d => d.CreatedDate, o => o.Ignore())
                .ForMember(d => d.CreatedBy, o => o.Ignore())
                .ForMember(d => d.CreatedByName, o => o.Ignore())
                .ForMember(d => d.CreatedIP, o => o.Ignore())
                .ForMember(d => d.ModifiedDate, o => o.Ignore())
                .ForMember(d => d.ModifiedBy, o => o.Ignore())
                .ForMember(d => d.ModifiedByName, o => o.Ignore())
                .ForMember(d => d.ModifiedIP, o => o.Ignore())
                .ForMember(d => d.Items, o => o.MapFrom(s => s.Items))
                .ForMember(d => d.Suppliers, o => o.MapFrom(s => s.Suppliers));

            CreateMap<RfqItemCreateDto, RfqItem>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.RfqId, o => o.Ignore())
                .ForMember(d => d.Quantity, o => o.MapFrom(s => s.Qty));

            CreateMap<RfqSupplierCreateDto, RfqSupplier>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.RfqId, o => o.Ignore())
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name ?? string.Empty))
                .ForMember(d => d.Email, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.Email) ? null! : new EmailAddress(s.Email!)))
                .ForMember(d => d.Mobile, o => o.MapFrom(s => s.Mobile))
                .ForMember(d => d.GSTNumber, o => o.MapFrom(s => s.Gst));

            CreateMap<RfqItem, RfqItemDto>()
                .ForCtorParam(nameof(RfqItemDto.ItemId), o => o.MapFrom(s => s.ItemId))
                .ForCtorParam(nameof(RfqItemDto.Qty), o => o.MapFrom(s => s.Quantity))
                .ForCtorParam(nameof(RfqItemDto.UomId), o => o.MapFrom(s => s.UomId))
                .ForCtorParam(nameof(RfqItemDto.HsnId), o => o.MapFrom(s => s.HsnId))
                .ForCtorParam(nameof(RfqItemDto.UomName), o => o.MapFrom(_ => string.Empty))
                .ForCtorParam(nameof(RfqItemDto.ItemName), o => o.MapFrom(_ => string.Empty))
                .ForCtorParam(nameof(RfqItemDto.GstPercentage), o => o.MapFrom(_ => 0m))
                .ForCtorParam(nameof(RfqItemDto.ItemCategoryId), o => o.MapFrom(_ => 0m));

            CreateMap<RfqSupplier, RfqSupplierDto>()
                .ForCtorParam(nameof(RfqSupplierDto.SupplierId), o => o.MapFrom(s => s.SupplierId))
                .ForCtorParam(nameof(RfqSupplierDto.Name), o => o.MapFrom(s => s.Name))
                .ForCtorParam(nameof(RfqSupplierDto.Email), o => o.MapFrom(s => s.Email != null ? s.Email.Value : null))
                .ForCtorParam(nameof(RfqSupplierDto.Mobile), o => o.MapFrom(s => s.Mobile))
                .ForCtorParam(nameof(RfqSupplierDto.Gst), o => o.MapFrom(s => s.GSTNumber));

            // ---------- READ: entity -> list dto ----------
           /*  CreateMap<RfqMaster, RfqListItemDto>()
                .ForCtorParam(nameof(RfqListItemDto.Id), o => o.MapFrom(s => s.Id))
                .ForCtorParam(nameof(RfqListItemDto.UnitId), o => o.MapFrom(s => s.UnitId))
                .ForCtorParam(nameof(RfqListItemDto.RfqCode), o => o.MapFrom(s => s.RfqCode))
                .ForCtorParam(nameof(RfqListItemDto.RfqStatusId), o => o.MapFrom(s => s.RfqStatusId))
                .ForCtorParam(nameof(RfqListItemDto.RfqStatusDesc), o => o.MapFrom(s => s.RfqStatus != null ? s.RfqStatus.Description : string.Empty))
                .ForCtorParam(nameof(RfqListItemDto.InitiationTypeId), o => o.MapFrom(s => s.InitiationTypeId))
                .ForCtorParam(nameof(RfqListItemDto.InitiationTypeDesc), o => o.MapFrom(s => s.InitiationType != null ? s.InitiationType.Description : string.Empty))
                .ForCtorParam(nameof(RfqListItemDto.IndentId), o => o.MapFrom(s => s.IndentId))
                .ForCtorParam(nameof(RfqListItemDto.LastSubmissionDate), o => o.MapFrom(s => s.LastSubmitDate))
                .ForCtorParam("Edit", o => o.MapFrom(s =>
                    s.QuotationRfq.Any(qh =>
                        qh.IsDeleted == DomainBase.IsDelete.NotDeleted &&
                        qh.IsActive == DomainBase.Status.Active) ? 1 : 0))
                .ForCtorParam("EditReason", o => o.MapFrom(s =>
                    s.QuotationRfq.Any(qh =>
                        qh.IsDeleted == DomainBase.IsDelete.NotDeleted &&
                        qh.IsActive == DomainBase.Status.Active)
                        ? "Quotation details exist" : null)) */
               CreateMap<RfqMaster, RfqListItemDto>()
                .ConvertUsing((s, ctx) =>
                    new RfqListItemDto(
                        s.Id,
                        s.UnitId,
                        s.RfqCode,
                        s.RfqStatusId,
                        s.RfqStatus != null ? s.RfqStatus.Description : string.Empty,
                        s.InitiationTypeId,
                        s.InitiationType != null ? s.InitiationType.Description : string.Empty,
                        s.IndentId,
                        s.LastSubmitDate,
                        s.QuotationRfq.Any(qh =>
                            qh.IsDeleted == DomainBase.IsDelete.NotDeleted &&
                            qh.IsActive  == DomainBase.Status.Active)
                            ? 1
                            : 0,
                        s.QuotationRfq.Any(qh =>
                            qh.IsDeleted == DomainBase.IsDelete.NotDeleted &&
                            qh.IsActive  == DomainBase.Status.Active)
                            ? "Quotation details exist"
                            : null,
                        GetRfqFlagStatus(s)   // now this is allowed (no expression tree)
                    )
                );





            CreateMap<IEnumerable<RfqItem>, RfqItemDto[]>().ConvertUsing((src, _, ctx) =>
                (src ?? Enumerable.Empty<RfqItem>()).Select(x => ctx.Mapper.Map<RfqItemDto>(x)).ToArray());

            CreateMap<IEnumerable<RfqSupplier>, RfqSupplierDto[]>().ConvertUsing((src, _, ctx) =>
                (src ?? Enumerable.Empty<RfqSupplier>()).Select(x => ctx.Mapper.Map<RfqSupplierDto>(x)).ToArray());

            CreateMap<RfqMaster, RfqDto>()
                .ForCtorParam(nameof(RfqDto.Id), o => o.MapFrom(s => s.Id))
                .ForCtorParam(nameof(RfqDto.UnitId), o => o.MapFrom(s => (int?)s.UnitId))
                .ForCtorParam(nameof(RfqDto.RfqCode), o => o.MapFrom(s => s.RfqCode))
                .ForCtorParam(nameof(RfqDto.RfqStatusId), o => o.MapFrom(s => s.RfqStatusId))
                .ForCtorParam(nameof(RfqDto.RfqStatusDesc), o => o.MapFrom(s => s.RfqStatus != null ? s.RfqStatus.Description : string.Empty))
                .ForCtorParam(nameof(RfqDto.InitiationTypeId), o => o.MapFrom(s => s.InitiationTypeId))
                .ForCtorParam(nameof(RfqDto.InitiationTypeDesc), o => o.MapFrom(s => s.InitiationType != null ? s.InitiationType.Description : string.Empty))
                .ForCtorParam(nameof(RfqDto.IndentId), o => o.MapFrom(s => (int?)s.IndentId))
                .ForCtorParam(nameof(RfqDto.LastSubmitDate), o => o.MapFrom(s => s.LastSubmitDate))
                .ForCtorParam(nameof(RfqDto.Items), o => o.MapFrom(s => s.Items))
                .ForCtorParam(nameof(RfqDto.Suppliers), o => o.MapFrom(s => s.Suppliers));

            // Autocomplete
            CreateMap<RfqMaster, RfqAutoCompleteDto>()
                .ForMember(d => d.RfqCode, o => o.MapFrom(s => s.RfqCode));
        }
        private static string GetRfqFlagStatus(RfqMaster rfq)
        {           
            var hasActiveQuotation = rfq.QuotationRfq.Any(qh =>
                qh.IsDeleted == DomainBase.IsDelete.NotDeleted &&
                qh.IsActive  == DomainBase.Status.Active);

            return hasActiveQuotation ? "Completed" : "Open";
        }
    }
}
