using AutoMapper;
using QCManagement.Application.QcInspection.Dto;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.Application.Common.Mappings
{
    public class QcInspectionProfile : Profile
    {
        public QcInspectionProfile()
        {
            // Entity → full DTO (cross-module names + parameters populated by the query repo)
            CreateMap<Domain.Entities.QcInspectionHdr, QcInspectionDto>()
                .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive == Status.Active))
                .ForMember(d => d.IsDeleted, o => o.MapFrom(s => s.IsDeleted == IsDelete.Deleted))
                .ForMember(d => d.SourceTypeCode, o => o.Ignore())
                .ForMember(d => d.SourceTypeName, o => o.Ignore())
                .ForMember(d => d.SourceNo, o => o.Ignore())
                .ForMember(d => d.SourceDate, o => o.Ignore())
                .ForMember(d => d.InvoiceNo, o => o.Ignore())
                .ForMember(d => d.SupplierId, o => o.Ignore())
                .ForMember(d => d.SupplierName, o => o.Ignore())
                .ForMember(d => d.ItemId, o => o.Ignore())
                .ForMember(d => d.ItemCode, o => o.Ignore())
                .ForMember(d => d.ItemName, o => o.Ignore())
                .ForMember(d => d.ItemCategoryId, o => o.Ignore())
                .ForMember(d => d.ItemCategoryName, o => o.Ignore())
                .ForMember(d => d.QcStatusCode, o => o.Ignore())
                .ForMember(d => d.QcStatusName, o => o.Ignore())
                .ForMember(d => d.Parameters, o => o.Ignore());
        }
    }
}
